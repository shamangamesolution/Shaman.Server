using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Shaman.Client.Peers
{
    public class ShamanConnectionStatus
    {
        public ShamanClientStatus Status;
        public string Error;
        public bool IsSuccess { get; set; }

        public ShamanConnectionStatus(ShamanClientStatus status, bool isSuccess = true,  string error = "")
        {
            Status = status;
            Error = error;
            IsSuccess = isSuccess;
        }
    }
    public enum ShamanClientStatus
    {
        Offline,
        ConnectingMatchMaking,
        AuthorizingMatchMaking,
        JoiningMatchMaking,
        OnMatchMaking,
        LeavingMatchMaking,
        ConnectingGameServer,
        AuthorizingGameServer,
        JoiningRoom,
        InRoom,
        LeavingRoom,
        JoinFailed,
        CreateGameError,
        Disconnected,
        ErrorReceived
    }
    
    public interface IShamanClientPeerListener
    {
        void OnStatusChanged(ShamanClientStatus prevStatus, ShamanClientStatus newStatus);
    }
    public class ShamanClientPeer : IShamanClientPeer
    {
        private readonly ClientPeer _clientPeer;
        private readonly IShamanLogger _logger;
        private readonly ITaskScheduler _taskScheduler;
        private readonly ISerializer _serializer;
        private ShamanClientStatus _status;
        private PendingTask _pollingTask;
        private readonly int _pollPackageQueueIntervalMs;
        private readonly IRequestSender _requestSender;
        private int _backendId;
        
        private readonly object _syncCollections = new object();
        private readonly Dictionary<ushort, Dictionary<Guid, EventHandler>> _handlers = new Dictionary<ushort, Dictionary<Guid, EventHandler>>();
        private readonly Dictionary<Type, ushort> _opCodesMap = new Dictionary<Type, ushort>();
        private readonly Dictionary<ushort, Func<byte[], int, int, MessageBase>> _parsers =
            new Dictionary<ushort, Func<byte[], int, int, MessageBase>>();
        private readonly Dictionary<Guid, ushort> _handlerIdToOperationCodes = new Dictionary<Guid, ushort>();
        
        private Dictionary<byte, object> _matchMakingProperties;
        private Dictionary<byte, object> _joinGameProperties;
        private Action<ShamanConnectionStatus, JoinInfo> _statusCallback;
        private Action<List<RoomInfo>> _getRoomsCallback;
        

        private Guid _joinInfoEventId;
        private PendingTask _pingTask;
        private PendingTask _resetPingTask;
        private bool _isPinging = false;
        private DateTime? _pingRequestSentOn = null;
        private JoinType _joinType;
        
        private int _rtt = int.MaxValue;
        //public Action<MessageBase> OnMessageReceived;
        
        public JoinInfo JoinInfo;
        public Guid SessionId;
        public Action<string> OnDisconnected { get; set; }
        public Action<string> OnDisconnectedFromMmServer { get; set; }
        public Action<string> OnDisconnectedFromGameServer { get; set; }

        public int Rtt => _rtt;

        private readonly IShamanClientPeerListener _listener;
        private static readonly TimeSpan JoinGameTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan ReceiveEventTimeout = TimeSpan.FromSeconds(5);

        #region ctors

        public ShamanClientPeer(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory,
            int pollPackageQueueIntervalMs, ISerializer serializer, IRequestSender requestSender,
            IShamanClientPeerListener listener, bool startOtherThreadMessageProcessing = true, int maxPacketSize = 300,
            int sendTickMs = 33)
        {
            _status = ShamanClientStatus.Offline;

            _logger = logger;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _serializer = serializer;
//            _serializer.InitializeDefaultSerializers(0, "client");
            _clientPeer = new ClientPeer(logger, taskSchedulerFactory, maxPacketSize, sendTickMs);
            _requestSender = requestSender;
            _listener = listener;
            _clientPeer.OnDisconnectedFromServer += (reason) =>
            {
                switch (_status)
                {
                    case ShamanClientStatus.ConnectingGameServer:
                    case ShamanClientStatus.AuthorizingGameServer:
                    case ShamanClientStatus.JoiningRoom:
                    case ShamanClientStatus.InRoom:
                    case ShamanClientStatus.LeavingRoom:
                        OnDisconnectedFromGameServer?.Invoke(reason);
                        break;
                    case ShamanClientStatus.ConnectingMatchMaking:
                    case ShamanClientStatus.AuthorizingMatchMaking:
                    case ShamanClientStatus.JoiningMatchMaking:
                    case ShamanClientStatus.OnMatchMaking:
                    case ShamanClientStatus.LeavingMatchMaking:
                        OnDisconnectedFromMmServer?.Invoke(reason);
                        break;
                }
                OnDisconnected?.Invoke(reason);
                SetAndReportStatus(ShamanClientStatus.Disconnected, _statusCallback, error: reason);
                ResetState();
            };
            _pollPackageQueueIntervalMs = pollPackageQueueIntervalMs;
            
            if (startOtherThreadMessageProcessing)
                StartProcessingMessagesLoop();
        }
        #endregion

        #region private and protected
        private void OnConnectedToMatchMaker(ConnectedEvent eve)
        {
            SetAndReportStatus(ShamanClientStatus.AuthorizingMatchMaking, _statusCallback);
                
            //authorizing matchmaker
            SendRequest<AuthorizationResponse>(new AuthorizationRequest(_backendId, SessionId), OmMmAuthorizationResponse);
        }

        private void GetRooms()
        {
            SendRequest<GetRoomListResponse>(new GetRoomListRequest(_matchMakingProperties), response =>
            {
                if (!response.Success)
                {
                    var error = $"GetRooms: Error response {response.Message}";
                    _logger.Error(error);
                    _getRoomsCallback(new List<RoomInfo>());
                    return;
                }

                _getRoomsCallback(response.Rooms);
            });
        }

        private void CreateGame()
        {
            SendRequest<CreateRoomFromClientResponse>(new CreateRoomFromClientRequest(_matchMakingProperties), response =>
            {
                if (!response.Success)
                {
                    var error = $"GetRooms: Error response {response.Message}";
                    _logger.Error(error);
                    SetAndReportStatus(ShamanClientStatus.CreateGameError, _statusCallback, false, error);
                    return;
                }
                
                JoinInfoReceived(response.JoinInfo);
            });
        }

        public void Connect(string address, ushort port)
        {
            _clientPeer.Connect(address, port);
        }
        
        private void StartConnect(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
            Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties,
            Action<ShamanConnectionStatus, JoinInfo> statusCallback)
        {
            try
            {
                _matchMakingProperties = matchMakingProperties;
                _joinGameProperties = joinGameProperties;
                _statusCallback = statusCallback;
                SessionId = sessionId;
                _backendId = backendId;
                //waiting for join Info
                _joinInfoEventId = RegisterOperationHandler<JoinInfoEvent>(OnJoinInfoEvent);

                SetAndReportStatus(ShamanClientStatus.ConnectingMatchMaking, statusCallback);
                RegisterOperationHandler<ConnectedEvent>(OnConnectedToMatchMaker, true);
                RegisterOperationHandler<ErrorResponse>(
                    errorResponse =>
                    {
                        SetAndReportStatus(ShamanClientStatus.ErrorReceived, _statusCallback, false,
                            errorResponse.ErrorCode.ToString());
                    }, false);

                Connect(matchMakerAddress, matchMakerPort);
            }
            catch (Exception ex)
            {
                _logger.Error($"JoinGame error: {ex}");
                throw new ShamanClientException("JoinGame error", ex);
            }
        }
        private void ProcessMessage(byte[] buffer, int offset, int length)
        {
            //calling registered handlers
            lock (_syncCollections)
            {
                var operationCode = MessageBase.GetOperationCode(buffer, offset);
                _logger.Debug($"Message received. Operation code: {operationCode}");

                if (operationCode != 0 && _handlers.ContainsKey(operationCode))
                {
                    var callbacksToUnregister = new List<Guid>();
                    if (!_parsers.TryGetValue(operationCode,out var parser))
                    {
                        throw new ShamanClientException($"No parser registered for operationCode {operationCode}");
                    }
                    
                    foreach (var item in _handlers[operationCode])
                    {
                        try
                        {
                            var messageBase = parser(buffer, offset, length);
                            item.Value.Handler.Invoke(messageBase);
                            if (item.Value.CallOnce)
                                callbacksToUnregister.Add(item.Key);
                        }
                        catch (Exception ex)
                        {
                            string targetName = item.Value == null ? "" : item.Value.Handler.Method.ToString();
                            var msg = $"ClientOnPackageReceived error: processing message {operationCode} in handler {targetName} {ex}";
                            _logger.Error(msg);
                            SetAndReportStatus(_status, _statusCallback, false, msg);
                        }
                    }
                    
                    //unregister
                    foreach(var item in callbacksToUnregister)
                        UnregisterOperationHandler(item);
                }
                else
                {
                    var msg = $"No handler for message {operationCode}";
                    _logger.Debug(msg);
                    //todo possible register handler for unsupported messages codes
                    // SetAndReportStatus(_status, _statusCallback, false, msg);
                }
            }
        }   
        //in unity this should be overriden to process messages inside the main thread
        protected virtual void StartProcessingMessagesLoop()
        {
            _pollingTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                IPacketInfo pack = null;
                while ((pack = PopNextPacket()) != null)
                {
                    //launch callback in another thread
                    var pack1 = pack;
                    _taskScheduler.ScheduleOnceOnNow(() => {ClientOnPackageReceived(pack1);});
                }
            }, 0, _pollPackageQueueIntervalMs);
        }

        public void ProcessMessages()
        {
            IPacketInfo pack = null;
            while ((pack = PopNextPacket()) != null)
            {
                //launch callback in another thread
                var pack1 = pack;
                ClientOnPackageReceived(pack1);
            }
        }

        #region join game flow
        private void SetAndReportStatus(ShamanClientStatus status, Action<ShamanConnectionStatus, JoinInfo> statusCallback = null, bool isSuccess = true, string error = "")
        {
            var prevStatus = _status;
            _status = status;
            statusCallback?.Invoke(new ShamanConnectionStatus(status, isSuccess, error), JoinInfo);
            if (prevStatus != _status)
            {
                _listener?.OnStatusChanged(prevStatus, status);    
            }
        }

        private void OmMmAuthorizationResponse(AuthorizationResponse response)
        {
            _logger.Debug($"JoinGame: AuthorizationRequest callback fired");
            if (response.ResultCode != ResultCode.OK)
            {
                _logger.Debug($"JoinGame: AuthorizationResponse error: {response.ResultCode}");
                SetAndReportStatus(ShamanClientStatus.AuthorizingMatchMaking, _statusCallback, false,
                    $"AuthorizationResponse error: {response.ResultCode}");
                return;
            }

            _logger.Debug($"JoinGame: Entering matchmaking");

            //calling next stage
            switch(_joinType)
            {
                case JoinType.RandomJoin:
                    _taskScheduler.ScheduleOnceOnNow(EnterMatchMaking);
                    break;
                case JoinType.DirectJoin:
                    _taskScheduler.ScheduleOnceOnNow(GetRooms);
                    break;
                case JoinType.CreateGame:
                    _taskScheduler.ScheduleOnceOnNow(CreateGame);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnEnterMatchmakingResponse(EnterMatchMakingResponse response)
        {
            if (response.ResultCode != ResultCode.OK)
            {
                SetAndReportStatus(ShamanClientStatus.JoiningMatchMaking, _statusCallback, false,
                    $"EnterMatchMakingResponse error: {response.ResultCode}");
            }
            else
            {
                if (response.MatchMakingErrorCode != MatchMakingErrorCode.OK)
                {
                    SetAndReportStatus(ShamanClientStatus.JoiningMatchMaking, _statusCallback, false,
                        $"EnterMatchMakingResponse error code: {response.MatchMakingErrorCode}");
                }
                else
                {
                    SetAndReportStatus(ShamanClientStatus.OnMatchMaking, _statusCallback);
                }
            }
        }

        private void OnPingResponse(MessageBase message)
        {
            var response = message as PingResponse;
            
            //drop reset task
            _taskScheduler.Remove(_resetPingTask);
            
            if (response != null && response.Success && _pingRequestSentOn != null)
            {
                _rtt = (int)((DateTime.UtcNow - _pingRequestSentOn.Value).TotalMilliseconds);
            }
            else
            {
                _rtt = int.MaxValue;
            }

            if (response != null && !response.Success)
            {
                _logger?.Error($"OnPingResponse error: {response.Message}");
            }

            _isPinging = false;
        }
        
        private void OnJoinRoomResponse(JoinRoomResponse response)
        {
            _logger.Debug($"JoinRoomResponse received");

            if (response.ResultCode != ResultCode.OK)
            {
                SetAndReportStatus(ShamanClientStatus.JoiningRoom, _statusCallback, false,
                    $"JoinRoomResponse error: {response.ResultCode}");
                return;
            }
                    
            SetAndReportStatus(ShamanClientStatus.InRoom, _statusCallback);
            
            //start ping sequence
            _pingTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                if (_isPinging)
                    return;
                
                _isPinging = true;
                _pingRequestSentOn = DateTime.UtcNow;
                SendRequest<PingResponse>(new PingRequest(), OnPingResponse);
                //schedule resetting flag
                _resetPingTask = _taskScheduler.Schedule(() =>
                {
                    _isPinging = false;
                    _rtt = int.MaxValue;
                }, 2000);
            }, 0, 1000);
        }

        private void OnGameAuthorizationResponse(AuthorizationResponse response)
        {
            if (response.ResultCode != ResultCode.OK)
            {
                SetAndReportStatus(ShamanClientStatus.AuthorizingGameServer, _statusCallback, false,
                    $"AuthorizationResponse error: {response.ResultCode}");
                return;
            }

            //calling next stage
            _taskScheduler.ScheduleOnceOnNow(JoinRoom);
        }

        private void JoinInfoReceived(JoinInfo joinInfo)
        {
            JoinInfo = joinInfo;
            _logger.Debug($"OnJoinInfoReceived: JoinInfo.Status {JoinInfo.Status}, JoinInfo.CurrentPlayers {JoinInfo.CurrentPlayers}, JoinInfo.MaxPlayers {JoinInfo.MaxPlayers}");
            
            SetAndReportStatus(_status, _statusCallback);
            
            //wait until we joined
            if (JoinInfo.Status == JoinStatus.RoomIsReady)
            {
                //start join room logic
                SetAndReportStatus(ShamanClientStatus.JoiningRoom, _statusCallback);

                //disconnecting
                _clientPeer.Disconnect();
                
                //connect to game server
                _taskScheduler.ScheduleOnceOnNow(() =>
                {
                    UnregisterOperationHandler(_joinInfoEventId);
                    ConnectToGameServer(JoinInfo.ServerIpAddress, JoinInfo.ServerPort);
                });
            }
        }
        
        private void OnJoinInfoEvent(JoinInfoEvent eve)
        {
            _logger.Debug($"OnJoinInfoReceived: JoinInfo event received");
            JoinInfoReceived(eve.JoinInfo);
        }

        private void ResetState()
        {
            _matchMakingProperties = null;
            _statusCallback = null;
            _status = ShamanClientStatus.Offline;
            _taskScheduler.Remove(_pingTask);
        }
        
        private void EnterMatchMaking()
        {
            try
            {
                //set correct status
                SetAndReportStatus(ShamanClientStatus.JoiningMatchMaking, _statusCallback);
                
                SendRequest<EnterMatchMakingResponse>(new EnterMatchMakingRequest(_matchMakingProperties), OnEnterMatchmakingResponse);
            }
            catch (Exception ex)
            {
                _logger.Error($"EnterMatchMaking error: {ex}");
            }
        }
        
        private void JoinRoom()
        {
            try
            {
                SetAndReportStatus(ShamanClientStatus.JoiningRoom, _statusCallback);

                SendRequest<JoinRoomResponse>(new JoinRoomRequest(JoinInfo.RoomId, _joinGameProperties), OnJoinRoomResponse);
            }
            catch (Exception ex)
            {
                _logger.Error($"JoinRoom error: {ex}");
            }
        }

        private void OnConnectedToGameServer(ConnectedEvent eve)
        {
            SetAndReportStatus(ShamanClientStatus.AuthorizingGameServer, _statusCallback);
                
            //authorizing matchmaker
            SendRequest<AuthorizationResponse>(new AuthorizationRequest(_backendId, SessionId), OnGameAuthorizationResponse);
        }
        
        private void ConnectToGameServer(string gameServerAddress, ushort gameServerPort)
        {
            try
            {
                SetAndReportStatus(ShamanClientStatus.ConnectingGameServer, _statusCallback);

                RegisterOperationHandler<ConnectedEvent>(OnConnectedToGameServer, true);
                
                _clientPeer.Connect(gameServerAddress, gameServerPort);
            }
            catch (Exception ex)
            {
                _logger.Error($"ConnectToGameServer error: {ex}");
            }
        }
        
        #endregion
        
        #endregion
        
        #region public

        public ShamanClientStatus GetStatus()
        {
            return _status;
        }

        public void ClientOnPackageReceived(IPacketInfo packet)
        {
            try
            {
                var offsets = PacketInfo.GetOffsetInfo(packet.Buffer, packet.Offset);
                foreach (var item in offsets)
                {
                    try
                    {
                        ProcessMessage(packet.Buffer, item.Offset, item.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error processing message: {ex}");
                    }
                }
            }
            finally
            {
                packet.Dispose();
            }
        }
        public Guid RegisterOperationHandler(ushort operationCode, Action<MessageBase> handler, bool callOnce = false)
        {
            _logger.Debug($"Registering OperationHandler {handler.Method} for operation {operationCode} (callOnce = {callOnce})");
            var id = Guid.NewGuid();
            lock (_syncCollections)
            {
                if (!_handlers.ContainsKey(operationCode))
                    _handlers.Add(operationCode, new Dictionary<Guid, EventHandler>());
        
                //add handler
                _handlers[operationCode].Add(id, new EventHandler(handler, callOnce));
                _handlerIdToOperationCodes[id] = operationCode;
            }
        
            return id;
        }

        public Guid RegisterOperationHandler<T>(Action<T> handler,
            bool callOnce = false) where T : MessageBase, new()
        {
            var id = Guid.NewGuid();
            lock (_syncCollections)
            {
                if (!_opCodesMap.TryGetValue(typeof(T), out var operationCode))
                {
                    operationCode = (new T()).OperationCode;
                    _opCodesMap.Add(typeof(T), operationCode);
                }

                _logger.Debug(
                    $"Registering OperationHandler {handler.Method} for operation {operationCode} (callOnce = {callOnce})");

                if (!_handlers.ContainsKey(operationCode))
                    _handlers.Add(operationCode, new Dictionary<Guid, EventHandler>());
                if (!_parsers.ContainsKey(operationCode))
                    _parsers.Add(operationCode, (data, offset, length) =>
                        _serializer.DeserializeAs<T>(data, offset, length));

                _handlers[operationCode].Add(id,
                    new EventHandler(msgBase => handler((T) msgBase), callOnce));
                _handlerIdToOperationCodes[id] = operationCode;
            }

            return id;
        }
        public void UnregisterOperationHandler(Guid id)
        {
            lock (_syncCollections)
            {
                if (!_handlerIdToOperationCodes.ContainsKey(id))
                    return;
                
                var operationCode = _handlerIdToOperationCodes[id];
                
                _logger.Debug($"Unregistering OperationHandler {id} for operation {operationCode}");

                //remove from maping
                _handlerIdToOperationCodes.Remove(id);

                if (!_handlers.ContainsKey(operationCode))
                    return;
                
                //remove from main collection
                if (!_handlers[operationCode].ContainsKey(id))
                    return;

                _handlers[operationCode].Remove(id);
            }
        }
        public IPacketInfo PopNextPacket()
        {
            return _clientPeer.PopNextPacket();
        }
        
        public void SendRequest<TResponse>(RequestBase request, Action<TResponse> callback) where TResponse: ResponseBase, new()
        {
            RegisterOperationHandler(callback, true);
            _taskScheduler.ScheduleOnceOnNow(() => _clientPeer.Send(request));
        }

        public Task<TResponse> SendRequest<TResponse>(RequestBase request) where TResponse : ResponseBase, new()
        {
            var task = new TaskCompletionSource<TResponse>();
            var cancellationTokenSource = new CancellationTokenSource(ReceiveEventTimeout);
            
            var handler = RegisterOperationHandler<TResponse>(response =>
            {
                if (response.Success)
                {
                    task.SetResult(response);
                }
                else
                {
                    task.SetException(new ShamanClientException($"Response for code {response.OperationCode} fail: {response.Message}"));
                }
                cancellationTokenSource.Dispose();
            }, true);

            cancellationTokenSource.Token.Register(() =>
            {
                task.TrySetCanceled();
                UnregisterOperationHandler(handler);
            });
            
            _taskScheduler.ScheduleOnceOnNow(() => _clientPeer.Send(request));
            return task.Task;
        }
        
        public void SendWebRequest<T>(string url, HttpRequestBase request, Action<T> callback)
            where T: HttpResponseBase,new()
        {
            _requestSender.SendRequest<T>(url, request, callback);
        }

        public Task<T> SendWebRequest<T>(string url, HttpRequestBase request)
            where T : HttpResponseBase, new()
        {
            return _requestSender.SendRequest<T>(url, request);
        }
        
        public void SendEvent(MessageBase eve)
        {
            _taskScheduler.ScheduleOnceOnNow(() => _clientPeer.Send(eve));
        }
        
        public Task<JoinInfo> JoinGame(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
            Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties)
        {
            _joinType = JoinType.RandomJoin;
            var joinTask = new TaskCompletionSource<JoinInfo>();
            var cancellationTokenSource = new CancellationTokenSource(JoinGameTimeout);
            cancellationTokenSource.Token.Register(() => joinTask.TrySetCanceled());

            StartConnect(matchMakerAddress, matchMakerPort, backendId, sessionId, matchMakingProperties,
                joinGameProperties, (status, info) =>
                {
                    if (joinTask.Task.IsCompleted)
                        return;
                    
                    if (!status.IsSuccess || status.Status == ShamanClientStatus.Disconnected)
                    {
                        joinTask.SetException(new ShamanClientException($"Client disconnected: {status.Error}"));
                        cancellationTokenSource.Dispose();
                    }
                    else if (info != null && info.Status == JoinStatus.MatchMakingFailed)
                    {
                        joinTask.SetException(new ShamanClientException("Matchmaking failed"));
                        cancellationTokenSource.Dispose();
                    }
                    else if (status.Status == ShamanClientStatus.InRoom)
                    {
                        joinTask.SetResult(info);
                        cancellationTokenSource.Dispose();
                    }
                });
            return joinTask.Task;
        }
        public void CreateGame(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
            Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties,
            Action<ShamanConnectionStatus, JoinInfo> statusCallback)
        {
            _joinType = JoinType.CreateGame;
            StartConnect(matchMakerAddress, matchMakerPort, backendId, sessionId, matchMakingProperties,
                joinGameProperties, statusCallback);
        }
        
        public void JoinRandomGame(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
            Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties,
            Action<ShamanConnectionStatus, JoinInfo> statusCallback)
        {
            _joinType = JoinType.RandomJoin;
            StartConnect(matchMakerAddress, matchMakerPort, backendId, sessionId, matchMakingProperties,
                joinGameProperties, statusCallback);
        }
        
        public void Disconnect()
        {
            _clientPeer.Disconnect();
            ResetState();
            OnDisconnected?.Invoke("Disconnect call");
        }

        public int GetSendQueueSize()
        {
            return _clientPeer.GetSendQueueLength();
        }
        #endregion
    }
}