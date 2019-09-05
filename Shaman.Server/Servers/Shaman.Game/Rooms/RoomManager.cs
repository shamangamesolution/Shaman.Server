using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Configuration;
using Shaman.Game.Rooms.GameModeControllers;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages;
using Shaman.Messages.General;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Messages.RoomFlow;

namespace Shaman.Game.Rooms
{
    public class RoomManager : IRoomManager
    {
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private readonly ConcurrentDictionary<Guid, IRoom> _rooms = new ConcurrentDictionary<Guid, IRoom>();
        private readonly ConcurrentDictionary<Guid, IRoom> _sessionsToRooms = new ConcurrentDictionary<Guid, IRoom>();
        private IShamanLogger _logger;
        private ISerializerFactory _serializerFactory;
        private object _syncPeersList = new object();
        private IApplicationConfig _config;
        private IRequestSender _requestSender;
        private IBackendProvider _backendProvider;
        private IGameModeControllerFactory _gameModeControllerFactory;
        private IRoomPropertiesContainer _roomPropertiesContainer;
        private IStorageContainer _storageContainer;
        private IPacketSender _packetSender;
        public RoomManager(
            IShamanLogger logger, 
            ISerializerFactory serializerFactory, 
            IApplicationConfig config, 
            ITaskSchedulerFactory taskSchedulerFactory, 
            IRequestSender requestSender, 
            IBackendProvider backendProvider, 
            IGameModeControllerFactory gameModeControllerFactory,
            IRoomPropertiesContainer roomPropertiesContainer,
            IStorageContainer storageContainer, IPacketSender packetSender)
        {
            _logger = logger;
            _serializerFactory = serializerFactory;
            _taskSchedulerFactory = taskSchedulerFactory;
            _requestSender = requestSender;
            _backendProvider = backendProvider;
            _gameModeControllerFactory = gameModeControllerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _taskScheduler.ScheduleOnInterval(CheckRoomsState, 0, 60000);
            _config = config;
            _taskSchedulerFactory = taskSchedulerFactory;
            _roomPropertiesContainer = roomPropertiesContainer;
            _storageContainer = storageContainer;
            _packetSender = packetSender;
        }

        private void CheckRoomsState()
        {
            lock (_syncPeersList)
            {
                _logger.Info($"Rooms state: Room count {GetRoomsCount()}, peers count {GetPlayersCount()}");
            }
        }
        
        /// <returns>IRoom?</returns>
        private IRoom GetRoomById(Guid id)
        {
            lock (_syncPeersList)
            {
                IRoom room;
                _rooms.TryGetValue(id, out room);
                return room;
            }
        }
        
        public IRoom GetRoomBySessionId(Guid sessionId)
        {
            lock (_syncPeersList)
            {
                IRoom room;
                _sessionsToRooms.TryGetValue(sessionId, out room);
                return room;
            }
        }
        
        public Guid CreateRoom(Dictionary<byte, object> properties,Dictionary<Guid, Dictionary<byte, object>> players)
        {
            lock (_syncPeersList)
            {
                _roomPropertiesContainer.Initialize(players, properties);

                var room = new Room(_logger, _taskSchedulerFactory, _requestSender, this, _backendProvider, _serializerFactory, _storageContainer, _roomPropertiesContainer, _gameModeControllerFactory, _packetSender);
                _rooms.TryAdd(room.GetRoomId(), room);
                //start room self destroy logic if rom is empty
                _taskScheduler.Schedule(() =>
                {
                    if (room.GetPeerCount() == 0)
                        DeleteRoom(room.GetRoomId());
                }, ((GameApplicationConfig)_config).DestroyEmptyRoomOnMs);
                return room.GetRoomId();
            }
        }

        public void DeleteRoom(Guid roomId)
        {
            lock (_syncPeersList)
            {
                var room = GetRoomById(roomId);
                room?.CleanUp();
                _rooms.TryRemove(roomId, out room);
            }
        }

        public List<IRoom> GetAllRooms()
        {
            lock (_syncPeersList)
            {
                return _rooms.Select(r => r.Value).ToList();
            }
        }

        public int GetRoomsCount()
        {
            lock (_syncPeersList)
            {
                return _rooms.Count;
            }
        }

        public int GetPlayersCount()
        {
            lock (_syncPeersList)
            {
                return _sessionsToRooms.Count;
            }
        }

        public void ConfirmedJoin(Guid sessionID, IRoom room)
        {
            _sessionsToRooms.TryAdd(sessionID, room);
        }
        
        public void PeerJoined(IPeer peer, Guid roomId, Dictionary<byte, object> peerProperties)
        {
            lock (_syncPeersList)
            {
                var room = GetRoomById(roomId);
                if (room == null)
                {
                    _logger.Error($"Peer {peer.GetSessionId()} attempted to join to non-exist room {roomId}");
                    _packetSender.AddPacket(new JoinRoomResponse() {ResultCode = ResultCode.RequestProcessingError}, peer);
                    return;
                }

                room.PeerJoined(peer, "", peerProperties);
            }
        }

        public bool IsInRoom(Guid sessionId)
        {
            lock (_syncPeersList)
            {
                return _sessionsToRooms.ContainsKey(sessionId);
            }
        }

        public void PeerLeft(Guid sessionId)
        {
            PeerDisconnected(sessionId);
        }

        public void PeerDisconnected(Guid sessionId)
        {
            lock (_syncPeersList)
            {
                var room = GetRoomBySessionId(sessionId);
                if (room == null)
                {
                    _logger.Error($"PeerDisconnected error: Can not get room for peer {sessionId}");
                    return;
                }

                room.PeerDisconnected(sessionId);
                _sessionsToRooms.TryRemove(sessionId, out var room1);

                if (room.GetPeerCount() == 0)
                {
                    DeleteRoom(room.GetRoomId());
                }
            }
        }

        public void ProcessMessage(MessageBase message, IPeer peer)
        {
            try
            {
                //room manager handles only room flow messages, others are sent to particular room
                switch (message.OperationCode)
                {
                    case CustomOperationCode.JoinRoom:
                        try
                        {
                            var joinMessage = message as JoinRoomRequest;
                            if (joinMessage == null)
                            {
                                _logger.Error($"Error casting to {typeof(JoinRoomRequest)}");
                                return;
                            }

                            PeerJoined(peer, joinMessage.RoomId, joinMessage.Properties);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"JoinRoom failed for player {peer.GetSessionId()}: {ex}");
                            _packetSender.AddPacket(new JoinRoomResponse() { ResultCode = ResultCode.RequestProcessingError }, peer);
                        }
                        break;
                    case CustomOperationCode.LeaveRoom:
                        PeerLeft(peer.GetSessionId());
                        break;
                    default:
                        var room = GetRoomBySessionId(peer.GetSessionId());
                        if (room == null)
                        {
                            _logger.Error($"ProcessMessage error: Can not get room for peer {peer.GetSessionId()}");
                            return;
                        }

                        room.ProcessMessage(message, peer.GetSessionId());
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"RoomManager.ProcessMessage: Error processing {message.OperationCode} message: {ex}");
            }

        }

        public Dictionary<Guid, int> GetRoomPeerCount()
        {
            lock (_syncPeersList)
            {
                return _rooms.ToDictionary(p => p.Key,
                    p => p.Value.GetPeerCount());
            }
        }

        public IRoom GetOldestRoom()
        {
            lock (_syncPeersList)
            {
                var oldestRoom = _rooms
                    .OrderByDescending(d => d.Value.GetCreatedOnDateTime())
                    .Select(e => (KeyValuePair<Guid, IRoom>?) e)
                    .FirstOrDefault();

                return oldestRoom?.Value;
            }
        }
    }
}