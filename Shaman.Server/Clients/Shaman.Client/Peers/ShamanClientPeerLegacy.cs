using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Client.Peers
{
    public class ConnectionStatusLegacy
    {
        public ClientStatusLegacy Status;
        public string Error;
        public bool IsSuccess { get; set; }

        public ConnectionStatusLegacy(ClientStatusLegacy status, bool isSuccess = true,  string error = "")
        {
            Status = status;
            Error = error;
            IsSuccess = isSuccess;
        }
    }

    public enum ClientStatusLegacy
    {
        Offline = 0,
        ConnectingMatchMaking = 1,
        AuthorizingMatchMaking = 2,
        JoiningMatchMaking = 3,
        OnMatchMaking = 4,
        LeavingMatchMaking = 5,
        ConnectingGameServer = 6,
        AuthorizingGameServer = 7,
        JoiningRoom = 8,
        InRoom = 9,
        LeavingRoom = 10,
        JoinFailed = 11,
        CreateGameError = 12
    }

    public class ShamanClientPeerLegacy
    {
        private readonly ClientPeer _clientPeer;
        private readonly IMessageDeserializer _messageDeserializer;
        private IShamanLogger _logger;
        private readonly ITaskScheduler _taskScheduler;
        private ISerializer _serializer;
        private ClientStatusLegacy _status;
        private IPendingTask _pollingTask;
        private readonly int _pollPackageQueueIntervalMs;
        private readonly IRequestSender _requestSender;
        private int _backendId;

        private object _syncCollections = new object();
        private Dictionary<ushort, Dictionary<Guid, EventHandler>> _handlers = new Dictionary<ushort, Dictionary<Guid, EventHandler>>();
        private readonly Dictionary<Guid, ushort> _handlerIdToOperationCodes = new Dictionary<Guid, ushort>();
        private Dictionary<byte, object> _matchMakingProperties;
        private Dictionary<byte, object> _joinGameProperties;
        private Action<ConnectionStatusLegacy, JoinInfo> _statusCallback;
        private Action<List<RoomInfo>> _getRoomsCallback;


        private Guid _joinInfoEventId;
        private IPendingTask _pingTask;
        private IPendingTask _resetPingTask;
        private bool _isPinging = false;
        private DateTime? _pingRequestSentOn = null;
        private JoinType _joinType;

        private int _rtt = int.MaxValue;
        //public Action<MessageBase> OnMessageReceived;

        public JoinInfo JoinInfo;
        public Guid SessionId;
        public Action<IDisconnectInfo> OnDisconnected;
        public Action<IDisconnectInfo> OnDisconnectedFromMmServer;
        public Action<IDisconnectInfo> OnDisconnectedFromGameServer;

        public int Rtt => _rtt;

        #region ctors

        public ShamanClientPeerLegacy(IMessageDeserializer messageDeserializer, IShamanLogger logger,
            ITaskSchedulerFactory taskSchedulerFactory, int pollPackageQueueIntervalMs, ISerializer serializer,
            IRequestSender requestSender, IClientTransportLayerFactory clientTransportLayerFactory, bool startOtherThreadMessageProcessing = true, int maxPacketSize = 300,
            int sendTickMs = 33)
        {
            _status = ClientStatusLegacy.Offline;

            _messageDeserializer = messageDeserializer;
            _logger = logger;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _serializer = serializer;
//            _serializer.InitializeDefaultSerializers(0, "client");
            _clientPeer = new ClientPeer(logger, clientTransportLayerFactory, taskSchedulerFactory, maxPacketSize,
                sendTickMs);
            _requestSender = requestSender;
            _clientPeer.OnDisconnectedFromServer += (reason) =>
            {
                switch (_status)
                {
                    case ClientStatusLegacy.ConnectingGameServer:
                    case ClientStatusLegacy.AuthorizingGameServer:
                    case ClientStatusLegacy.JoiningRoom:
                    case ClientStatusLegacy.InRoom:
                    case ClientStatusLegacy.LeavingRoom:
                        OnDisconnectedFromGameServer?.Invoke(reason);
                        break;
                    case ClientStatusLegacy.ConnectingMatchMaking:
                    case ClientStatusLegacy.AuthorizingMatchMaking:
                    case ClientStatusLegacy.JoiningMatchMaking:
                    case ClientStatusLegacy.OnMatchMaking:
                    case ClientStatusLegacy.LeavingMatchMaking:
                        OnDisconnectedFromMmServer?.Invoke(reason);
                        break;
                }
                OnDisconnected?.Invoke(reason);
                ResetState();
            };
            _pollPackageQueueIntervalMs = pollPackageQueueIntervalMs;

            if (startOtherThreadMessageProcessing)
                StartProcessingMessagesLoop();
        }
        #endregion

        #region private and protected
        private void OnConnectedToMatchMaker(MessageBase eve)
        {
            SetAndReportStatus(ClientStatusLegacy.AuthorizingMatchMaking, _statusCallback);

            //authorizing matchmaker
            SendRequest(new AuthorizationRequest {SessionId = SessionId}, OmMmAuthorizationResponse);
        }

        private void GetRooms()
        {
            SendRequest(new GetRoomListRequest(_matchMakingProperties), message =>
            {
                var response = message as GetRoomListResponse;
                if (response == null)
                {
                    var error = "GetRooms: Error casting GetRoomListResponse";
                    _logger.Error(error);
                    _getRoomsCallback(new List<RoomInfo>());
                    return;
                }

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
            SendRequest(new CreateRoomFromClientRequest(_matchMakingProperties), message =>
            {
                var response = message as CreateRoomFromClientResponse;
                if (response == null)
                {
                    var error = "GetRooms: Error casting CreateRoomFromClientResponse";
                    _logger.Error(error);
                    SetAndReportStatus(ClientStatusLegacy.CreateGameError, _statusCallback, false, error);
                    return;
                }

                if (!response.Success)
                {
                    var error = $"GetRooms: Error response {response.Message}";
                    _logger.Error(error);
                    SetAndReportStatus(ClientStatusLegacy.CreateGameError, _statusCallback, false, error);
                    return;
                }

                JoinInfoReceived(response.JoinInfo);
            });
        }

        public void PingConnect(string address, ushort port, Action<bool> callback, int timeoutMs = 1000)
        {
            var handlerId = RegisterOperationHandler(ShamanOperationCode.Connect, (msg) =>
            {
                Disconnect();
                callback(true);
            }, true);
            Connect(address, port);
            var task = _taskScheduler.Schedule(() =>
            {
                UnregisterOperationHandler(handlerId);
                callback(false);
            }, timeoutMs);
        }

        public void Connect(string address, ushort port)
        {
            _clientPeer.Connect(address, port);
        }

        private void StartConnect(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
            Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties,
            Action<ConnectionStatusLegacy, JoinInfo> statusCallback)
        {
            try
            {
                _matchMakingProperties = matchMakingProperties;
                _joinGameProperties = joinGameProperties;
                _statusCallback = statusCallback;
                SessionId = sessionId;
                _backendId = backendId;
                //waiting for join Info
                _joinInfoEventId = RegisterOperationHandler(ShamanOperationCode.JoinInfo, OnJoinInfoEvent);

                SetAndReportStatus(ClientStatusLegacy.ConnectingMatchMaking, statusCallback);
                RegisterOperationHandler(ShamanOperationCode.Connect, OnConnectedToMatchMaker, true);
                Connect(matchMakerAddress, matchMakerPort);
            }
            catch (Exception ex)
            {
                _logger.Error($"JoinGame error: {ex}");
            }
        }
        private void ProcessMessage(byte[] buffer, int offset, int length)
        {
            MessageBase deserialized = null;
            ushort operationCode = 0;
            try
            {
                //probably bad kind of using
                var message = new ArraySegment<byte>(buffer, offset, length).ToArray();

                operationCode = MessageBase.GetOperationCode(message);

                _logger.Debug($"Message received. Operation code: {operationCode}");

                deserialized = _messageDeserializer.DeserializeMessage(operationCode, _serializer, message);
            }
            catch (Exception ex)
            {
                _logger.Error($"ClientOnPackageReceived error: {ex}");
            }

            //calling registered handlers
            lock (_syncCollections)
            {
                if (deserialized != null && operationCode != 0 && _handlers.ContainsKey(operationCode))
                {
                    var callbacksToUnregister = new List<Guid>();
                    foreach (var item in _handlers[operationCode])
                    {
                        try
                        {
                            item.Value.Handler.Invoke(deserialized);
                            if (item.Value.CallOnce)
                                callbacksToUnregister.Add(item.Key);
                        }
                        catch (Exception ex)
                        {
                            string targetName = item.Value == null ? "" : item.Value.Handler.Method.ToString();
                            _logger.Error($"ClientOnPackageReceived error: processing message {deserialized.OperationCode} in handler {targetName} {ex}");
                        }
                    }

                    //unregister
                    foreach(var item in callbacksToUnregister)
                        UnregisterOperationHandler(item);
                }
                else
                {
                    _logger.Debug($"No handler for message {operationCode}");
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



        #region join game flow
        private void SetAndReportStatus(ClientStatusLegacy status, Action<ConnectionStatusLegacy, JoinInfo> statusCallback = null, bool isSuccess = true, string error = "")
        {
            _status = status;
            statusCallback?.Invoke(new ConnectionStatusLegacy(status, isSuccess, error), JoinInfo);
        }

        private void OmMmAuthorizationResponse(MessageBase message)
        {
            _logger.Debug($"JoinGame: AuthorizationRequest callback fired");
            var response = message as AuthorizationResponse;
            if (response == null)
            {
                _logger.Debug($"JoinGame: Error casting AuthorizationResponse");
                SetAndReportStatus(ClientStatusLegacy.AuthorizingMatchMaking, _statusCallback, false,
                    "Error casting AuthorizationResponse");
                return;
            }

            if (response.ResultCode != ResultCode.OK)
            {
                _logger.Debug($"JoinGame: AuthorizationResponse error: {response.ResultCode}");
                SetAndReportStatus(ClientStatusLegacy.AuthorizingMatchMaking, _statusCallback, false,
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
                // case JoinType.DirectJoin:
                //     _taskScheduler.ScheduleOnceOnNow(GetRooms);
                //     break;
                // case JoinType.CreateGame:
                //     _taskScheduler.ScheduleOnceOnNow(CreateGame);
                //     break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnEnterMatchmakingResponse(MessageBase message)
        {
            var response = message as EnterMatchMakingResponse;
            if (response == null || response.ResultCode != ResultCode.OK)
            {
                if (response == null)
                {
                    SetAndReportStatus(ClientStatusLegacy.JoiningMatchMaking, _statusCallback, false,
                        "Error casting EnterMatchMakingResponse");
                    return;
                }

                if (response.ResultCode != ResultCode.OK)
                {
                    SetAndReportStatus(ClientStatusLegacy.JoiningMatchMaking, _statusCallback, false,
                        $"EnterMatchMakingResponse error: {response.ResultCode}");
                    return;
                }
            }
            else
            {
                SetAndReportStatus(ClientStatusLegacy.OnMatchMaking, _statusCallback);
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

        private void OnJoinRoomResponse(MessageBase message)
        {
            _logger.Debug($"JoinRoomResponse received");
            var response = message as JoinRoomResponse;

            if (response == null || response.ResultCode != ResultCode.OK)
            {
                if (response == null)
                {
                    SetAndReportStatus(ClientStatusLegacy.JoiningRoom, _statusCallback, false,
                        "Error casting JoinRoomResponse");
                    return;
                }

                if (response.ResultCode != ResultCode.OK)
                {
                    SetAndReportStatus(ClientStatusLegacy.JoiningRoom, _statusCallback, false,
                        $"JoinRoomResponse error: {response.ResultCode}");
                    return;
                }
            }

            SetAndReportStatus(ClientStatusLegacy.InRoom, _statusCallback);

            //start ping sequence
            _pingTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                if (_isPinging)
                    return;

                _isPinging = true;
                _pingRequestSentOn = DateTime.UtcNow;
                SendRequest(new PingRequest(), OnPingResponse);
                //schedule resetting flag
                _resetPingTask = _taskScheduler.Schedule(() =>
                {
                    _isPinging = false;
                    _rtt = int.MaxValue;
                }, 2000);
            }, 0, 1000);
        }

        private void OnGameAuthorizationResponse(MessageBase message)
        {
            var response = message as AuthorizationResponse;
            if (response == null)
            {
                SetAndReportStatus(ClientStatusLegacy.AuthorizingGameServer, _statusCallback, false,
                    "Error casting AuthorizationResponse");
                return;
            }

            if (response.ResultCode != ResultCode.OK)
            {
                SetAndReportStatus(ClientStatusLegacy.AuthorizingGameServer, _statusCallback, false,
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
                SetAndReportStatus(ClientStatusLegacy.JoiningRoom, _statusCallback);

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

        private void OnJoinInfoEvent(MessageBase joinInfoMessage)
        {
            _logger.Debug($"OnJoinInfoReceived: JoinInfo event received");
            var eve = joinInfoMessage as JoinInfoEvent;
            if (eve == null)
            {
                _logger.Debug($"OnJoinInfoReceived: Error casting JoinInfoEvent");
                SetAndReportStatus(_status, _statusCallback, false, $"Error casting JoinInfoEvent");
                return;
            }

            JoinInfoReceived(eve.JoinInfo);
        }

        private void ResetState()
        {
            _matchMakingProperties = null;
            _statusCallback = null;
            _status = ClientStatusLegacy.Offline;
            _taskScheduler.Remove(_pingTask);
        }

        private void EnterMatchMaking()
        {
            try
            {
                //set correct status
                SetAndReportStatus(ClientStatusLegacy.JoiningMatchMaking, _statusCallback);

                SendRequest(new EnterMatchMakingRequest(_matchMakingProperties), OnEnterMatchmakingResponse);
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
                SetAndReportStatus(ClientStatusLegacy.JoiningRoom, _statusCallback);

                SendRequest(new JoinRoomRequest(JoinInfo.RoomId, _joinGameProperties), OnJoinRoomResponse);
            }
            catch (Exception ex)
            {
                _logger.Error($"JoinRoom error: {ex}");
            }
        }

        private void OnConnectedToGameServer(MessageBase eve)
        {
            SetAndReportStatus(ClientStatusLegacy.AuthorizingGameServer, _statusCallback);

            //authorizing matchmaker
            SendRequest(new AuthorizationRequest {SessionId = SessionId}, OnGameAuthorizationResponse);
        }

        private void ConnectToGameServer(string gameServerAddress, ushort gameServerPort)
        {
            try
            {
                SetAndReportStatus(ClientStatusLegacy.ConnectingGameServer, _statusCallback);

                RegisterOperationHandler(ShamanOperationCode.Connect, OnConnectedToGameServer, true);

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

        public ClientStatusLegacy GetStatus()
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
        public void SendRequest(RequestBase request, Action<MessageBase> callback)
        {
            RegisterOperationHandler(request.OperationCode, callback, true);
            Send(request);
        }
        public void SendWebRequest<T>(string url, HttpRequestBase request, Action<T> callback)
            where T: HttpResponseBase,new()
        {
            _requestSender.SendRequest<T>(url, request, callback);
        }

        public void SendEvent(EventBase eve)
        {
            Send(eve);
        }

        private void Send(MessageBase eve)
        {
            _taskScheduler.ScheduleOnceOnNow(() => _clientPeer.Send(eve, eve.IsReliable, eve.IsOrdered));
        }

        public void JoinGame(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
            Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties,
            Action<ConnectionStatusLegacy, JoinInfo> statusCallback)
        {
            _joinType = JoinType.RandomJoin;
            StartConnect(matchMakerAddress, matchMakerPort, backendId, sessionId, matchMakingProperties,
                joinGameProperties, statusCallback);
        }

        public void GetGames(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
            Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties,
            Action<List<RoomInfo>> getRoomsCallback)
        {
            _getRoomsCallback = getRoomsCallback;
            _joinType = JoinType.DirectJoin;
            StartConnect(matchMakerAddress, matchMakerPort, backendId, sessionId, matchMakingProperties,
                joinGameProperties, null);
        }

        public void JoinGame(Guid roomId, Action<ConnectionStatusLegacy, JoinInfo> statusCallback)
        {
            _statusCallback = statusCallback;
            SendRequest(new DirectJoinRequest(roomId, _matchMakingProperties), message =>
            {
                var response = message as DirectJoinResponse;
                if (response == null)
                {
                    var error = "JoinRoom: Error casting DirectJoinResponse";
                    _logger.Error(error);
                    SetAndReportStatus(ClientStatusLegacy.JoinFailed, statusCallback, false, error);
                    return;
                }

                if (!response.Success)
                {
                    var error = $"JoinRoom: Error response {response.Message}";
                    _logger.Error(error);
                    SetAndReportStatus(ClientStatusLegacy.JoinFailed, statusCallback, false, error);
                    return;
                }

                JoinInfoReceived(response.JoinInfo);
            });
        }

        // public void CreateGame(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
        //     Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties,
        //     Action<ConnectionStatusLegacy, JoinInfo> statusCallback)
        // {
        //     _joinType = JoinType.CreateGame;
        //     StartConnect(matchMakerAddress, matchMakerPort, backendId, sessionId, matchMakingProperties,
        //         joinGameProperties, statusCallback);
        // }
        //
        // public void JoinRandomGame(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId,
        //     Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties,
        //     Action<ConnectionStatusLegacy, JoinInfo> statusCallback)
        // {
        //     _joinType = JoinType.RandomJoin;
        //     StartConnect(matchMakerAddress, matchMakerPort, backendId, sessionId, matchMakingProperties,
        //         joinGameProperties, statusCallback);
        // }

        public void Disconnect()
        {
            _clientPeer.Disconnect();
            ResetState();
            OnDisconnected?.Invoke(new SimpleDisconnectInfo(ShamanDisconnectReason.PeerLeave));
        }

        public int GetSendQueueSize()
        {
            return _clientPeer.GetSendQueueLength();
        }
        #endregion
    }
}