using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Shaman.Client.Peers
{
    public class ConnectionStatus
    {
        public ClientStatus Status;
        public string Error;
        public bool IsSuccess { get; set; }

        public ConnectionStatus(ClientStatus status, bool isSuccess = true,  string error = "")
        {
            Status = status;
            Error = error;
            IsSuccess = isSuccess;
        }
    }
    
    public enum ClientStatus
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
    }

    public class EventHandler
    {
        public Action<MessageBase> Handler;
        public bool CallOnce;

        public EventHandler(Action<MessageBase> handler, bool callOnce)
        {
            Handler = handler;
            CallOnce = callOnce;
        }
    }
    
    public class ShamanClientPeer 
    {
        private ClientPeer _clientPeer = null;
        private IShamanLogger _logger;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private ISerializerFactory _serializerFactory;
        private ClientStatus _status;
        private PendingTask _pollingTask;
        private int _pollPackageQueueIntervalMs;
        private IRequestSender _requestSender;
        private int _backendId;
        
        private object _syncCollections = new object();
        private Dictionary<ushort, Dictionary<Guid, EventHandler>> _handlers = new Dictionary<ushort, Dictionary<Guid, EventHandler>>();
        private Dictionary<Guid, ushort> _handlerIdToOperationCodes = new Dictionary<Guid, ushort>(); 
        private Dictionary<byte, object> _matchMakingProperties;
        private Dictionary<byte, object> _joinGameProperties;
        private Action<ConnectionStatus, JoinInfo> _statusCallback;

        private Guid _joinInfoEventId;
        private Guid _pingTaskId, _resetPingTaskId;
        private bool _isPinging = false;
        private DateTime? _pingRequestSentOn = null;

        private int _rtt = int.MaxValue;
        //public Action<MessageBase> OnMessageReceived;
        
        public JoinInfo JoinInfo;
        public Guid SessionId;
        public Action<string> OnDisconnected;
        public Action<string> OnDisconnectedFromMmServer;
        public Action<string> OnDisconnectedFromGameServer;

        public int Rtt => _rtt;

        #region ctors
        public ShamanClientPeer(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, int pollPackageQueueIntervalMs, ISerializerFactory serializerFactory, IRequestSender requestSender, bool startOtherThreadMessageProcessing = true, int maxPacketSize = 300, int sendTickMs = 33)
        {
            _status = ClientStatus.Offline;
            
            _logger = logger;
            _taskSchedulerFactory = taskSchedulerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _serializerFactory = serializerFactory;
            _serializerFactory.InitializeDefaultSerializers(0, "client");
            _clientPeer = new ClientPeer(logger, _taskSchedulerFactory, maxPacketSize, sendTickMs);
            _requestSender = requestSender;
            _clientPeer.OnDisconnectedFromServer += (reason) =>
            {
                switch (_status)
                {
                    case ClientStatus.ConnectingGameServer:
                    case ClientStatus.AuthorizingGameServer:
                    case ClientStatus.JoiningRoom:
                    case ClientStatus.InRoom:
                    case ClientStatus.LeavingRoom:
                        OnDisconnectedFromGameServer?.Invoke(reason);
                        break;
                    case ClientStatus.ConnectingMatchMaking:
                    case ClientStatus.AuthorizingMatchMaking:
                    case ClientStatus.JoiningMatchMaking:
                    case ClientStatus.OnMatchMaking:
                    case ClientStatus.LeavingMatchMaking:
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
        //in unity this should be overriden to process messages inside the main thread
        protected virtual void StartProcessingMessagesLoop()
        {
            _pollingTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                PacketInfo pack = null;
                while ((pack = PopNextPacket()) != null)
                {
                    //launch callback in another thread
                    var pack1 = pack;
                    _taskScheduler.ScheduleOnceOnNow(() => {ClientOnPackageReceived(pack1);});
                }
            }, 0, _pollPackageQueueIntervalMs);
        }


        
        #region join game flow
        private void SetAndReportStatus(ClientStatus status, Action<ConnectionStatus, JoinInfo> statusCallback = null, bool isSuccess = true, string error = "")
        {
            _status = status;
            statusCallback?.Invoke(new ConnectionStatus(status, isSuccess, error), JoinInfo);
        }

        private void OmMmAuthorizationResponse(MessageBase message)
        {
            _logger.Debug($"JoinGame: AuthorizationRequest callback fired");
            var response = message as AuthorizationResponse;
            if (response == null)
            {
                _logger.Debug($"JoinGame: Error casting AuthorizationResponse");
                SetAndReportStatus(ClientStatus.AuthorizingMatchMaking, _statusCallback, false,
                    "Error casting AuthorizationResponse");
                return;
            }

            if (response.ResultCode != ResultCode.OK)
            {
                _logger.Debug($"JoinGame: AuthorizationResponse error: {response.ResultCode}");
                SetAndReportStatus(ClientStatus.AuthorizingMatchMaking, _statusCallback, false,
                    $"AuthorizationResponse error: {response.ResultCode}");
                return;
            }

            _logger.Debug($"JoinGame: Entering matchmaking");

            //calling next stage
            _taskScheduler.ScheduleOnceOnNow(EnterMatchMaking);
        }

        private void OnEnterMatchmakingResponse(MessageBase message)
        {
            var response = message as EnterMatchMakingResponse;
            if (response == null || response.ResultCode != ResultCode.OK)
            {
                if (response == null)
                {
                    SetAndReportStatus(ClientStatus.JoiningMatchMaking, _statusCallback, false,
                        "Error casting EnterMatchMakingResponse");
                    return;
                }

                if (response.ResultCode != ResultCode.OK)
                {
                    SetAndReportStatus(ClientStatus.JoiningMatchMaking, _statusCallback, false,
                        $"EnterMatchMakingResponse error: {response.ResultCode}");
                    return;
                }
            }
            else
            {
                SetAndReportStatus(ClientStatus.OnMatchMaking, _statusCallback);
            }
        }

        private void OnPingResponse(MessageBase message)
        {
            var response = message as PingResponse;
            
            //drop reset task
            _taskScheduler.Remove(_resetPingTaskId);
            
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
                    SetAndReportStatus(ClientStatus.JoiningRoom, _statusCallback, false,
                        "Error casting JoinRoomResponse");
                    return;
                }

                if (response.ResultCode != ResultCode.OK)
                {
                    SetAndReportStatus(ClientStatus.JoiningRoom, _statusCallback, false,
                        $"JoinRoomResponse error: {response.ResultCode}");
                    return;
                }
            }
                    
            SetAndReportStatus(ClientStatus.InRoom, _statusCallback);
            
            //start ping sequence
            _pingTaskId = _taskScheduler.ScheduleOnInterval(() =>
            {
                if (_isPinging)
                    return;
                
                _isPinging = true;
                _pingRequestSentOn = DateTime.UtcNow;
                SendRequest(new PingRequest(), OnPingResponse);
                //schedule resetting flag
                _resetPingTaskId = _taskScheduler.Schedule(() =>
                {
                    _isPinging = false;
                    _rtt = int.MaxValue;
                }, 2000).Id;
            }, 0, 1000).Id;
        }

        private void OnGameAuthorizationResponse(MessageBase message)
        {
            var response = message as AuthorizationResponse;
            if (response == null)
            {
                SetAndReportStatus(ClientStatus.AuthorizingGameServer, _statusCallback, false,
                    "Error casting AuthorizationResponse");
                return;
            }

            if (response.ResultCode != ResultCode.OK)
            {
                SetAndReportStatus(ClientStatus.AuthorizingGameServer, _statusCallback, false,
                    $"AuthorizationResponse error: {response.ResultCode}");
                return;
            }

            //calling next stage
            _taskScheduler.ScheduleOnceOnNow(JoinRoom);
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

            JoinInfo = eve.JoinInfo;
            _logger.Debug($"OnJoinInfoReceived: JoinInfo.Status {JoinInfo.Status}, JoinInfo.CurrentPlayers {JoinInfo.CurrentPlayers}, JoinInfo.MaxPlayers {JoinInfo.MaxPlayers}");
            
            SetAndReportStatus(_status, _statusCallback);
            
            //wait until we joined
            if (JoinInfo.Status == JoinStatus.RoomIsReady)
            {
                //start join room logic
                SetAndReportStatus(ClientStatus.JoiningRoom, _statusCallback);

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

        private void ResetState()
        {
            _matchMakingProperties = null;
            _statusCallback = null;
            _status = ClientStatus.Offline;
            _taskScheduler.Remove(_pingTaskId);
        }
        
        private void EnterMatchMaking()
        {
            try
            {
                //set correct status
                SetAndReportStatus(ClientStatus.JoiningMatchMaking, _statusCallback);
                
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
                SetAndReportStatus(ClientStatus.JoiningRoom, _statusCallback);

                SendRequest(new JoinRoomRequest(JoinInfo.RoomId, _joinGameProperties), OnJoinRoomResponse);
            }
            catch (Exception ex)
            {
                _logger.Error($"JoinRoom error: {ex}");
            }
        }

        private void OnConnectedToGameServer(MessageBase eve)
        {
            SetAndReportStatus(ClientStatus.AuthorizingGameServer, _statusCallback);
                
            //authorizing matchmaker
            SendRequest(new AuthorizationRequest(_backendId, SessionId), OnGameAuthorizationResponse);
        }
        
        private void ConnectToGameServer(string gameServerAddress, ushort gameServerPort)
        {
            try
            {
                SetAndReportStatus(ClientStatus.ConnectingGameServer, _statusCallback);

                RegisterOperationHandler(CustomOperationCode.Connect, OnConnectedToGameServer, true);
                
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

        public ClientStatus GetStatus()
        {
            return _status;
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

                deserialized = MessageFactory.DeserializeMessage(operationCode, _serializerFactory, message);
                
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

        public void ClientOnPackageReceived(PacketInfo packet)
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
            
            packet.RecycleCallback?.Invoke();
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
        public PacketInfo PopNextPacket()
        {
            return _clientPeer.PopNextPacket();
        }
        public void SendRequest(RequestBase request, Action<MessageBase> callback)
        {
            RegisterOperationHandler(request.OperationCode, callback, true);
            _taskScheduler.ScheduleOnceOnNow(() => _clientPeer.Send(request));
        }
        public void SendWebRequest<T>(string url, RequestBase request, Action<T> callback)
            where T: ResponseBase,new()
        {
            _requestSender.SendRequest<T>(url, request, callback);
        }
        public void SendEvent(EventBase eve)
        {
            _taskScheduler.ScheduleOnceOnNow(() => _clientPeer.Send(eve));
        }
        
        private void OnConnectedToMatchMaker(MessageBase eve)
        {
            SetAndReportStatus(ClientStatus.AuthorizingMatchMaking, _statusCallback);
                
            //authorizing matchmaker
            SendRequest(new AuthorizationRequest(_backendId, SessionId), OmMmAuthorizationResponse);
        }
        public void JoinGame(string matchMakerAddress, ushort matchMakerPort, int backendId, Guid sessionId, Dictionary<byte, object> matchMakingProperties, Dictionary<byte, object> joinGameProperties, Action<ConnectionStatus, JoinInfo> statusCallback)
        {
            try
            {
                _matchMakingProperties = matchMakingProperties;
                _joinGameProperties = joinGameProperties;
                _statusCallback = statusCallback;
                SessionId = sessionId;
                _backendId = backendId;
                //waiting for join Info
                _joinInfoEventId = RegisterOperationHandler(CustomOperationCode.JoinInfo, OnJoinInfoEvent);
                
                SetAndReportStatus(ClientStatus.ConnectingMatchMaking, statusCallback);
                RegisterOperationHandler(CustomOperationCode.Connect, OnConnectedToMatchMaker, true);
                
                _clientPeer.Connect(matchMakerAddress, matchMakerPort);
            }
            catch (Exception ex)
            {
                _logger.Error($"JoinGame error: {ex}");
            }
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