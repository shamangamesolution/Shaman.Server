using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Configuration;
using Shaman.Game.Contract;
using Shaman.Game.Contract.Stats;
using Shaman.Game.Metrics;
using Shaman.Game.Providers;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.Messages;
using Shaman.Messages.RoomFlow;

namespace Shaman.Game.Rooms
{
    public class RoomManager : IRoomManager
    {
        private readonly ITaskSchedulerFactory _taskSchedulerFactory;
        private readonly ITaskScheduler _taskScheduler;
        private readonly ConcurrentDictionary<Guid, IRoom> _rooms = new ConcurrentDictionary<Guid, IRoom>();
        private readonly ConcurrentDictionary<Guid, IRoom> _sessionsToRooms = new ConcurrentDictionary<Guid, IRoom>();
        private readonly IShamanLogger _logger;
        private readonly ISerializer _serializer;
        private readonly object _syncPeersList = new object();
        private readonly IApplicationConfig _config;
        private readonly IGameModeControllerFactory _gameModeControllerFactory;
        private readonly IPacketSender _packetSender;
        private readonly IGameMetrics _gameMetrics;
        private readonly IRequestSender _requestSender;
        private static readonly TimeSpan RoomLivingTimeAfterGameEnd = TimeSpan.FromMinutes(15);

        public RoomManager(
            IShamanLogger logger, 
            ISerializer serializer, 
            IApplicationConfig config, 
            ITaskSchedulerFactory taskSchedulerFactory, 
            IGameModeControllerFactory gameModeControllerFactory,
            IPacketSender packetSender, IGameMetrics gameMetrics, IRequestSender requestSender)
        {
            _logger = logger;
            _serializer = serializer;
            _taskSchedulerFactory = taskSchedulerFactory;
            _gameModeControllerFactory = gameModeControllerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _taskScheduler.ScheduleOnInterval(CheckRoomsState, 0, 60000);
            _config = config;
            _taskSchedulerFactory = taskSchedulerFactory;
            _packetSender = packetSender;
            _gameMetrics = gameMetrics;
            _requestSender = requestSender;
        }

        private void CheckRoomsState()
        {
            foreach (var room in _rooms)
            {
                var roomValue = room.Value;
                if (DateTime.UtcNow - (roomValue.GetCreatedOnDateTime() + roomValue.GetRoomTtl()) >
                    RoomLivingTimeAfterGameEnd)
                {
                    _logger?.Error($"Room destroy with peers count {roomValue.GetPeerCount()}");
                    DeleteRoom(roomValue.GetRoomId());
                }
            }

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
        
        
        
        public Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            lock (_syncPeersList)
            {
                var roomPropertiesContainer = new RoomPropertiesContainer(_logger);
                var packetSender = new PacketBatchSender(_taskSchedulerFactory, _config, _serializer);
                
                packetSender.Start(true);
                
                roomPropertiesContainer.Initialize(players, properties);

                var room = new Room(_logger, _taskSchedulerFactory, this, _serializer, roomPropertiesContainer, _gameModeControllerFactory, packetSender, _requestSender);

                if(_rooms.TryAdd(room.GetRoomId(), room))
                    _gameMetrics.TrackRoomCreated();
                //start room self destroy logic if room is empty
                _taskScheduler.Schedule(() =>
                {
                    var peerCount = room.GetPeerCount();
                    if (peerCount == 0)
                    {
                        DeleteRoom(room.GetRoomId());
                    }
                }, ((GameApplicationConfig)_config).DestroyEmptyRoomOnMs);
                return room.GetRoomId();
            }
        }

        public void UpdateRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            lock (_syncPeersList)
            {
                var room = GetRoomById(roomId);
                if (room != null)
                {
                    room.UpdateRoom(players);
                }
                else
                {
                    _logger?.Error($"UpdateRoom error: can not find room {roomId} to add new players");
                    return;
                }
            }
        }

        public void DeleteRoom(Guid roomId)
        {
            lock (_syncPeersList)
            {
                var room = GetRoomById(roomId);
                var roomStats = room?.GetStats();
                room?.CleanUp();
                if (_rooms.TryRemove(roomId, out room))
                    TrackRoomMetricsOnDelete(roomStats);
            }
        }

        private void TrackRoomMetricsOnDelete(RoomStats roomStats)
        {
            _gameMetrics.TrackRoomDestroyed();
            if (roomStats != null)
            {
                _gameMetrics.TrackMaxSendQueueSize(roomStats.GetMaxQueueSize());
                _gameMetrics.TrackAvgSendQueueSize((int) (roomStats.GetAvgQueueSize() * 100));
                
                _gameMetrics.TrackRoomTotalTrafficSent(roomStats.TotalTrafficSent);
                _gameMetrics.TrackRoomTotalTrafficReceived(roomStats.TotalTrafficReceived);
                _gameMetrics.TrackTotalRoomLiveTime(roomStats.TotalLiveTimeSec);
                var messageStatistics = roomStats.BuildMessageStatistics();

                _gameMetrics.TrackRoomTotalMessagesReceived(messageStatistics.Received.Sum(m => m.Item3));
                _gameMetrics.TrackRoomTotalMessagesSent(messageStatistics.Sent.Sum(m => m.Item3));

                foreach (var item in messageStatistics.Received)
                {
                    _gameMetrics.TrackRoomTotalMessagesReceived(item.Item3, item.Item1.ToString());
                }

                foreach (var item in messageStatistics.Sent)
                {
                    _gameMetrics.TrackRoomTotalMessagesSent(item.Item3, item.Item1.ToString());
                }
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
                    var message = $"Peer {peer.GetSessionId()} attempted to join to non-exist room {roomId}";
                    _logger.Error(message);
                    throw new Exception(message);
                }

                if (room.PeerJoined(peer, peerProperties))
                {
                    _gameMetrics.TrackPeerJoin();
                }
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
                _gameMetrics.TrackPeerDisconnected();

                if (room.GetPeerCount() == 0)
                {
                    DeleteRoom(room.GetRoomId());
                }
            }
        }

        public void ProcessMessage(ushort operationCode, MessageData message, IPeer peer)
        {
            try
            {
                //room manager handles only room flow messages, others are sent to particular room
                switch (operationCode)
                {
                    case CustomOperationCode.JoinRoom:
                        try
                        {
                            var joinMessage = _serializer.DeserializeAs<JoinRoomRequest>(message.Buffer, message.Offset, message.Length);
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

                        room.ProcessMessage(operationCode, message, peer.GetSessionId());
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"RoomManager.ProcessMessage: Error processing {operationCode} message: {ex}");
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