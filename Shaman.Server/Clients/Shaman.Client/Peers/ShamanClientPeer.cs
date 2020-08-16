using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Client.Peers.MessageHandling;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.Router.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Client.Peers
{
    public interface IShamanClientPeerListener
    {
        void OnStatusChanged(ShamanClientStatus prevStatus, ShamanClientStatus newStatus);
    }
    public class ShamanClientPeer : IShamanClientPeer
    {
        private readonly ClientPeer _clientPeer;
        private readonly IShamanLogger _logger;
        private readonly ITaskScheduler _taskScheduler;
        private ShamanClientStatus _status;
        private IPendingTask _pollingTask;
        private readonly int _pollPackageQueueIntervalMs;
        private readonly IRequestSender _requestSender;
        private int _backendId;

        private Dictionary<byte, object> _matchMakingProperties;
        private Dictionary<byte, object> _joinGameProperties;
        private Action<ShamanConnectionStatus, JoinInfo> _statusCallback;
        private Action<List<RoomInfo>> _getRoomsCallback;
        

        private Guid _joinInfoEventId;
        private bool _isPinging = false;
        private DateTime? _pingRequestSentOn = null;
        private JoinType _joinType;
        
        //public Action<MessageBase> OnMessageReceived;
        
        public JoinInfo JoinInfo;
        public Guid SessionId;
        public Action<string> OnDisconnected { get; set; }
        public Action<string> OnDisconnectedFromMmServer { get; set; }
        public Action<string> OnDisconnectedFromGameServer { get; set; }

        public int GetRtt()
        {
            if (_clientPeer == null)
                return 0;

            return _clientPeer.GetRtt();
        }
        
        public int GetPing()
        {
            if (_clientPeer == null)
                return 0;

            return _clientPeer.GetPing();
        }

        private readonly IShamanClientPeerListener _listener;
        private static readonly TimeSpan JoinGameTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan ReceiveEventTimeout = TimeSpan.FromSeconds(5);
        private readonly IMessageHandler _shamanMessageHandler;
        private readonly IMessageHandler _bundleMessageHandler;

        #region ctors

        public ShamanClientPeer(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory,
            ISerializer serializer, IRequestSender requestSender,
            IShamanClientPeerListener listener, IShamanClientPeerConfig config)
        {
            _status = ShamanClientStatus.Offline;

            _shamanMessageHandler = new MessageHandler(logger, serializer);
            _bundleMessageHandler = new MessageHandler(logger, serializer);

            _logger = logger;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
//            _serializer.InitializeDefaultSerializers(0, "client");
            _clientPeer = new ClientPeer(logger, taskSchedulerFactory, config.MaxPacketSize, config.SendTickMs);
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
            _pollPackageQueueIntervalMs = config.PollPackageQueueIntervalMs;
            
            if (config.StartOtherThreadMessageProcessing)
                StartProcessingMessagesLoop();
        }
        #endregion

        #region private and protected
        private void OnConnectedToMatchMaker(ConnectedEvent eve)
        {
            SetAndReportStatus(ShamanClientStatus.AuthorizingMatchMaking, _statusCallback);
                
            //authorizing matchmaker
            SendShamanRequest<AuthorizationResponse>(new AuthorizationRequest(_backendId, SessionId), OmMmAuthorizationResponse);
        }

        private void GetRooms()
        {
            SendShamanRequest<GetRoomListResponse>(new GetRoomListRequest(_matchMakingProperties), response =>
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
            SendShamanRequest<CreateRoomFromClientResponse>(new CreateRoomFromClientRequest(_matchMakingProperties), response =>
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

                _clientPeer.Connect(matchMakerAddress, matchMakerPort);
            }
            catch (Exception ex)
            {
                _logger.Error($"JoinGame error: {ex}");
                throw new ShamanClientException("JoinGame error", ex);
            }
        }

        private void StartProcessingMessagesLoop()
        {
            _pollingTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                IPacketInfo pack = null;
                while ((pack = PopNextPacket()) != null)
                {
                    //launch callback in another thread
                    var pack1 = pack;
                    _taskScheduler.ScheduleOnceOnNow(() => { ClientOnPackageReceived(pack1); });
                }
            }, 0, _pollPackageQueueIntervalMs);
        }

        public void ProcessMessages()
        {
            IPacketInfo pack;
            while ((pack = PopNextPacket()) != null)
            {
                //launch callback in another thread
                ClientOnPackageReceived(pack);
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
        
        public void Connect(string address, ushort port)
        {
            _clientPeer.Connect(address, port);
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
            
            //wait until we joinedÚ
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
        }
        
        private void EnterMatchMaking()
        {
            try
            {
                //set correct status
                SetAndReportStatus(ShamanClientStatus.JoiningMatchMaking, _statusCallback);
                
                SendShamanRequest<EnterMatchMakingResponse>(new EnterMatchMakingRequest(_matchMakingProperties), OnEnterMatchmakingResponse);
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

                SendShamanRequest<JoinRoomResponse>(new JoinRoomRequest(JoinInfo.RoomId, _joinGameProperties), OnJoinRoomResponse);
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
            SendShamanRequest<AuthorizationResponse>(new AuthorizationRequest(_backendId, SessionId), OnGameAuthorizationResponse);
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
        
        public Task<JoinInfo> DirectConnectToGameServer(string gameServerAddress, ushort gameServerPort, Guid sessionId,  Guid roomId, Dictionary<byte, object> joinGameProperties)
        {
            _joinGameProperties = joinGameProperties;
            SessionId = sessionId;
            
            var joinTask = new TaskCompletionSource<JoinInfo>();
            var cancellationTokenSource = new CancellationTokenSource(JoinGameTimeout);
            cancellationTokenSource.Token.Register(() => joinTask.TrySetCanceled());
            
            JoinInfo = new JoinInfo(gameServerAddress, gameServerPort, roomId, JoinStatus.RoomIsReady, 1, 1);
            
            _statusCallback = (status, info) =>
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
            };
            
            SetAndReportStatus(ShamanClientStatus.ConnectingGameServer, _statusCallback);
            
            RegisterOperationHandler<ConnectedEvent>(OnConnectedToGameServer, true);
            
            _clientPeer.Connect(gameServerAddress, gameServerPort);
            
            return joinTask.Task;
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
                        var operationCode = MessageBase.GetOperationCode(packet.Buffer, item.Offset);
                        if (operationCode == ShamanOperationCode.Bundle)
                        {
                            _logger.Debug($"Shaman message received. Operation code: {operationCode}");
                            _shamanMessageHandler.ProcessMessage(operationCode, packet.Buffer, item.Offset, item.Length);
                        }
                        else
                        {
                            _logger.Debug($"Bundle message received. Operation code: {operationCode}");
                            var bundleOperationCode = MessageBase.GetOperationCode(packet.Buffer, item.Offset + 1);
                            _bundleMessageHandler.ProcessMessage(bundleOperationCode, packet.Buffer, item.Offset + 1, item.Length - 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        SetAndReportStatus(_status, _statusCallback, false, ex.Message);
                    }
                }
            }
            finally
            {
                packet.Dispose();
            }
        }

        public IPacketInfo PopNextPacket()
        {
            return _clientPeer.PopNextPacket();
        }

        private void SendShamanRequest<TResponse>(RequestBase request, Action<TResponse> callback) where TResponse: ResponseBase, new()
        {
            RegisterShamanOperationHandler(callback, true);
            SendShamanEvent(request);
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
            
            SendEvent(request);
            return task.Task;
        }

        public Guid RegisterOperationHandler<T>(Action<T> handler, bool callOnce = false) where T : MessageBase, new()
        {
            return _bundleMessageHandler.RegisterOperationHandler(handler, callOnce);
        }

        private Guid RegisterShamanOperationHandler<T>(Action<T> handler, bool callOnce = false) where T : MessageBase, new()
        {
            return _shamanMessageHandler.RegisterOperationHandler(handler, callOnce);
        }

        public void UnregisterOperationHandler(Guid id)
        {
            _bundleMessageHandler.UnregisterOperationHandler(id);
        }

        private void UnregisterShamanOperationHandler(Guid id)
        {
            _shamanMessageHandler.UnregisterOperationHandler(id);
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
        
        private void SendShamanEvent(MessageBase eve)
        {
            _taskScheduler.ScheduleOnceOnNow(() => _clientPeer.Send(eve, eve.IsReliable, eve.IsOrdered));
        }
        
        public void SendEvent<TMessage>(TMessage eve) where TMessage : MessageBase
        {
            _taskScheduler.ScheduleOnceOnNow(() =>
                _clientPeer.Send(new BundleMessageWrapper<TMessage>(eve), eve.IsReliable, eve.IsOrdered));
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

        public async Task<int> Ping(Route route, int timeoutMs = 500)
        {
            var timer = Stopwatch.StartNew();
            var ping = 0;
            try
            {
                _logger.Debug($"ConnectTo: connecting with {route.MatchMakerAddress}:{route.MatchMakerPort}. TimeOut {timeoutMs}");
                var task = new TaskCompletionSource<object>();
                var cancellationTokenSource = new CancellationTokenSource(timeoutMs);
                _clientPeer.OnConnectedToServer = () =>
                {
                    _logger.Debug($"ConnectTo: ConnectEvent received");
                    if (task.Task.IsCompleted)
                        return;
                    task.SetResult(task);
                    cancellationTokenSource.Dispose();
                };
                cancellationTokenSource.Token.Register(() =>
                {
                    _logger.Debug($"ConnectTo: Timeout token fired...");
                    task.TrySetCanceled();
                });
                _clientPeer.Connect(route.MatchMakerAddress, route.MatchMakerPort);
                await (Task) task.Task;
                ping = _clientPeer.GetPing();
            }
            finally
            {
                Disconnect();
            }

            return ping;//(int) timer.ElapsedMilliseconds;
        }

        public int GetMessagesCountInQueue()
        {
            return _clientPeer.GetMessagesCountInQueue();
        }
    }
}