using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Peers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Rooms.GameModeControllers;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages;
using Shaman.Messages.General;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Messages.Stats;

namespace Shaman.Game.Rooms
{
    public class Room : IRoom
    {
        private IShamanLogger _logger;
        private readonly ConcurrentDictionary<Guid, RoomPlayer> _roomPlayers = new ConcurrentDictionary<Guid, RoomPlayer>();

        private Guid _roomId;
        private DateTime _createdOn;
        //private Dictionary<byte, object> _properties;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private IRequestSender _requestSender;
        private IRoomManager _roomManager;
        private IBackendProvider _backendProvider;
        private ISerializerFactory _serializerFactory;
        private IRoomPropertiesContainer _roomPropertiesContainer;
        private IGameModeControllerFactory _gameModeControllerFactory;

        private IPacketSender _packetSender;
        private IGameModeController _gameModeController = null;
        private RoomStats _roomStats;
        private Guid _roomStatTaskId, _testTaskId;
        public Room(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IRequestSender requestSender, IRoomManager roomManager, IBackendProvider backendProvider, ISerializerFactory serializerFactory, IStorageContainer storageContainer, IRoomPropertiesContainer roomPropertiesContainer, IGameModeControllerFactory gameModeControllerFactory, IPacketSender packetSender)
        {
            _logger = logger;
            _roomId = Guid.NewGuid();
            _createdOn = DateTime.UtcNow;
            _taskSchedulerFactory = taskSchedulerFactory;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _requestSender = requestSender;
            _roomManager = roomManager;
            _backendProvider = backendProvider;
            _serializerFactory = serializerFactory;
            _roomPropertiesContainer = roomPropertiesContainer;
            _gameModeControllerFactory = gameModeControllerFactory;
            _packetSender = packetSender;

            
            _roomStats = new RoomStats(GetRoomId(), _roomPropertiesContainer.GetPlayersCount());
            
            //init game mode controller
            if (!_roomPropertiesContainer.IsRoomPropertiesContainsKey(PropertyCode.RoomProperties.GameMode))
            {
                _logger.Error($"There is no GameMode property while creating room");
                return;
            }

            //setup gameMode controller
            _gameModeController =
                _gameModeControllerFactory.GetGameModeController(
                    (GameMode) _roomPropertiesContainer.GetRoomProperty<byte>(PropertyCode.RoomProperties.GameMode), this, _taskScheduler);

            _roomStatTaskId = _taskScheduler.ScheduleOnInterval(() =>
            {
                _roomStats.MaxSendQueueSize.Add(_packetSender.GetMaxQueueSIze());
                _roomStats.AverageQueueSize.Add(_packetSender.GetAverageQueueSize());
                
            }, 0, 1000).Id;
        }
        
        public void SendToAll(MessageBase message, List<Guid> exceptions = null)
        {
            if (exceptions == null)
                exceptions = new List<Guid>();

            var initMsgArray = message.Serialize(_serializerFactory);
//            var buf = _bufferPool.Get(initMsgArray.Length);
//            Array.Copy(initMsgArray, 0, buf, 0, initMsgArray.Length);
            
            foreach (var player in _roomPlayers.Where(p => !exceptions.Exists(e => e == p.Value.Peer.GetSessionId())))
            {
                AddToSendQueue(initMsgArray, player.Value.Peer, message.IsReliable, message.IsOrdered);
                //add to stats
                _roomStats.AddSentMessage(message.OperationCode, initMsgArray.Length, message.IsReliable);
            }
        }
        
        public void DisposeRoom()
        {
            //destroy this room
            _roomManager.DeleteRoom(_roomId);
        }

        public List<RoomPlayer> GetAllPlayers()
        {
            return _roomPlayers.Select(r => r.Value).ToList();
        }

        public void ConfirmedJoin(Guid sessionId)
        {
            _roomManager.ConfirmedJoin(sessionId, this);
        }

        public RoomStats GetStats()
        {
            return _roomStats;
        }

        public Guid GetRoomId()
        {
            return _roomId;
        }

        public bool PeerJoined(IPeer peer, string secret, Dictionary<byte, object> peerProperties)
        {
            if (!_roomPlayers.TryAdd(peer.GetSessionId(), new RoomPlayer(peer, peerProperties)))
            {
                _logger.Error($"Error adding player to peer collection");
                return false;
            }

            try
            {
                //find player and set wasjoined
                _gameModeController?.ProcessNewPlayer(peer.GetSessionId(), peerProperties);
                if (_gameModeController == null)
                {
                    _logger.Error($"GameModeController == null while peer joining");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"PeerJoined error for player with sessionId = {peer.GetSessionId()}: {ex}");
                return false;
            }

        }

        public void PeerLeft(Guid sessionId)
        {
            _roomPlayers.TryRemove(sessionId, out var player);
            try
            {
                _gameModeController.CleanupPlayer(sessionId);
            }
            catch (Exception ex)
            {
                _logger.Error($"CleanUpPlayer error: {ex}");
            }
        }

        public void PeerDisconnected(Guid sessionId)
        {
            _roomPlayers.TryRemove(sessionId, out var player);
        }

        public void AddToSendQueue(MessageBase message, Guid sessionId)
        {
            if (!_roomPlayers.ContainsKey(sessionId))
                return;
            
            var player = _roomPlayers[sessionId];
            if (player != null)
            {
                var serialized = message.Serialize(_serializerFactory);
                AddToSendQueue(serialized, player.Peer, message.IsReliable, message.IsOrdered);
                //add to stats
                _roomStats.AddSentMessage(message.OperationCode, serialized.Length, message.IsReliable);
                //_taskScheduler.ScheduleOnceOnNow(() => player.Peer.Send(serialized, ));
            }
            else
            {
                _logger.Error($"Trying to send message {message.GetType()} to non-existing player {sessionId}");
            }
        }
        
        public void AddToSendQueue(byte[] bytes, IPeer peer, bool isReliable, bool isOrdered)
        {
            //_taskScheduler.ScheduleOnceOnNow(() => peer.Send(bytes, isReliable, isOrdered));
            _taskScheduler.ScheduleOnceOnNow(() =>
            {
                _packetSender.AddPacket(peer, bytes, isReliable, isOrdered);
            });

        }
        
        public void ProcessMessage(MessageBase message, Guid sessionId)
        {
            try
            {
                var result = true;
                _roomStats.AddReceivedMessage(message);
                
                //process room message
                switch (message.OperationCode)
                {
                    case CustomOperationCode.PingRequest:
                        AddToSendQueue(new PingResponse(), sessionId);
                        break;
                    case CustomOperationCode.Test:
                        break;
                    default:
                        result = _gameModeController.ProcessMessage(message, sessionId);
                        break;
                }

                if (message.IsBroadcasted && result)
                {
                    SendToAll(message, new List<Guid> {sessionId});
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"Room.ProcessMessage: Error processing {message.OperationCode} message: {ex}");
            }
        }

        public void CleanUp()
        {
            _logger?.Error($"RoomStats: {_roomStats}");
            foreach (var player in _roomPlayers)
            {
                player.Value.Peer.Disconnect(DisconnectReason.RoomCleanup);
            }
            _taskScheduler.RemoveAll();
            _roomPlayers.Clear();
        }

        public int GetPeerCount()
        {
            return _roomPlayers.Count;
        }

        public DateTime GetCreatedOnDateTime()
        {
            return _createdOn;
        }

        public RoomPlayer GetPlayer(Guid sessionId)
        {            
            _roomPlayers.TryGetValue(sessionId, out var player);
            return player;
        }
    }
}