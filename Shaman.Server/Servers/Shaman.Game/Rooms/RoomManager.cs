using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms.Exceptions;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.Game.Stats;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.RoomFlow;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

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
        private readonly IRoomControllerFactory _roomControllerFactory;
        private readonly IShamanMessageSender _messageSender;
        private readonly IGameMetrics _gameMetrics;
        private readonly IRoomStateUpdater _roomStateUpdater;
        private readonly Random _rnd = new Random();
        
        public RoomManager(
            IShamanLogger logger,
            ISerializer serializer,
            IApplicationConfig config,
            ITaskSchedulerFactory taskSchedulerFactory,
            IRoomControllerFactory roomControllerFactory,
            IPacketSender packetSender, 
            IShamanMessageSenderFactory messageSenderFactory,
            IGameMetrics gameMetrics, 
            IRoomStateUpdater roomStateUpdater)
        {
            _logger = logger;
            _serializer = serializer;
            _taskSchedulerFactory = taskSchedulerFactory;
            _roomControllerFactory = roomControllerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _taskScheduler.ScheduleOnInterval(CheckRoomsState, 0, 60000);
            _config = config;
            _taskSchedulerFactory = taskSchedulerFactory;
            _messageSender = messageSenderFactory.Create(packetSender);
            _gameMetrics = gameMetrics;
            _roomStateUpdater = roomStateUpdater;
        }

        private void CheckRoomsState()
        {
            foreach (var room in _rooms)
            {
                var roomValue = room.Value;
                if (roomValue.IsGameFinished() || IsTimeToForceRoomDestroy(roomValue))
                {
                    _logger.Error($"Room destroy with peers count {roomValue.GetPeerCount()}");
                    DeleteRoom(roomValue.GetRoomId());
                }
            }

            _logger.Info($"Rooms state: Room count {GetRoomsCount()}, peers count {GetPlayersCount()}");
        }

        private static bool IsTimeToForceRoomDestroy(IRoom roomValue)
        {
            var destroyRoomAfter = roomValue.ForceDestroyRoomAfter;
            return destroyRoomAfter != TimeSpan.MaxValue && DateTime.UtcNow - roomValue.GetCreatedOnDateTime() >
                   destroyRoomAfter;
        }

        private IRoom GetRandomRoomOrCreateOne(Guid sessionId, Dictionary<byte, object> roomProperties)
        {
            IRoom result = null;
            var openRooms = _rooms.Where(r =>
                r.Value.IsOpen() && DictionaryHelpers.AreDictionariesEqual(roomProperties,
                    r.Value.GetPropertiesContainer().GetRoomProperties())).Select(i => i.Value).ToList();
            var players = new Dictionary<Guid,Dictionary<byte, object>> {{sessionId, new Dictionary<byte, object>()}};
            if (!openRooms.Any())
            {
                var roomId = CreateRoom(roomProperties, players, null);
                result = GetRoomById(roomId);
            }
            else
            {
                result = openRooms[_rnd.Next(0, openRooms.Count)];
                UpdateRoom(result.GetRoomId(), players);
            }

            return result;
        }
        
        public IRoom GetRoomById(Guid id)
        {
            _rooms.TryGetValue(id, out var room);
            return room;
        }

        public IRoom GetRoomBySessionId(Guid sessionId)
        {
            _sessionsToRooms.TryGetValue(sessionId, out var room);
            return room;
        }

        public Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players, Guid? roomId)
        {
            lock (_syncPeersList)
            {
                if (roomId.HasValue && _rooms.ContainsKey(roomId.Value))
                {
                    var message = $"Failed to create room: room with specified id already exists ({roomId})";
                    _logger.Error(message);
                    throw new Exception(message);
                }
                
                var roomPropertiesContainer = new RoomPropertiesContainer(_logger);
                var packetSender = new PacketBatchSender(_taskSchedulerFactory, _config, _logger);
                
                packetSender.Start(true);
                
                roomPropertiesContainer.Initialize(players, properties);

                var room = new Room(_logger, _taskSchedulerFactory, this, roomPropertiesContainer,
                    _roomControllerFactory, packetSender, roomId ?? Guid.NewGuid(), _roomStateUpdater);

                if(_rooms.TryAdd(room.GetRoomId(), room))
                    _gameMetrics.TrackRoomCreated();
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

        private void DeleteRoom(Guid roomId)
        {
            lock (_syncPeersList)
            {
                if (!_rooms.TryGetValue(roomId, out var room)) 
                    return;
                
                var roomStats = room.GetStats();
                _gameMetrics.TrackPeerDisconnected(room.CleanUp());
                _taskScheduler.ScheduleOnceOnNow(async () => await room.InvalidateRoom());
                _rooms.TryRemove(roomId, out _);
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
            return _rooms.Select(r => r.Value).ToList();
        }

        public int GetRoomsCount()
        {
            return _rooms.Count;
        }

        private int GetPlayersCount()
        {
            return _sessionsToRooms.Count;
        }

        private void LinkSessionToRoom(Guid sessionId, IRoom room)
        {
            _sessionsToRooms.TryAdd(sessionId, room);
        }
        public bool IsInRoom(Guid sessionId)
        {
            return _sessionsToRooms.ContainsKey(sessionId);
        }

        public void PeerDisconnected(IPeer peer, IDisconnectInfo info)
        {
            lock (_syncPeersList)
            {
                var sessionId = peer.GetSessionId();
                if (!_sessionsToRooms.TryGetValue(sessionId, out var room))
                {
                    _logger.Error($"PeerDisconnected error: Can not get room for peer {sessionId}");
                    return;
                }

                if (room.PeerDisconnected(sessionId, info))
                    _gameMetrics.TrackPeerDisconnected();
                
                _sessionsToRooms.TryRemove(sessionId, out _);
                _messageSender.CleanupPeerData(peer);

                if (room.IsGameFinished())
                {
                    DeleteRoom(room.GetRoomId());
                }
            }
        }

        private void JoinRoomAndReport<T>(IRoom roomToJoin, Guid sessionId, IPeer peer, Dictionary<byte, object> joinProperties, T successResponse, T errorResponse)
            where T:ResponseBase
        {
            LinkSessionToRoom(sessionId, roomToJoin);
            if (!roomToJoin.AddPeerToRoom(peer, joinProperties))
            {
                _messageSender.Send(errorResponse, peer);
                return;
            }
                            
            _gameMetrics.TrackPeerJoin();
            _taskScheduler.ScheduleOnceOnNow(async () =>
            {
                try
                {
                    if (await roomToJoin.PeerJoined(peer, joinProperties))
                    {
                        _messageSender.Send(successResponse, peer);
                    }
                    else
                    {
                        // peer.Disconnect(DisconnectReason.JustBecause);
                        _messageSender.Send(errorResponse, peer);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"JoinRoom failed for player {peer.GetSessionId()}: {ex}");
                    _messageSender.Send(errorResponse, peer);
                }
            });
        }
        
        public void ProcessMessage(ushort operationCode, Payload message, DeliveryOptions deliveryOptions,
            IPeer peer)
        {
            try
            {
                //room manager handles only room flow messages, others are sent to particular room
                var sessionId = peer.GetSessionId();

                switch (operationCode)
                {
                    case ShamanOperationCode.JoinRandomRoom:
                        var joinRandomRoomMessage = _serializer.DeserializeAs<DirectJoinRandomRoomRequest>(message.Buffer, message.Offset, message.Length);
                        var roomToRandomJoin = GetRandomRoomOrCreateOne(sessionId, joinRandomRoomMessage.RoomProperties);
                        
                        if (roomToRandomJoin == null)
                        {
                            var msg = $"Error joining random room";
                            _logger.Error(msg);
                            _messageSender.Send(new JoinRoomResponse() { ResultCode = ResultCode.RequestProcessingError }, peer);
                        }
                        else
                        {
                            JoinRoomAndReport(roomToRandomJoin, sessionId, peer, joinRandomRoomMessage.JoinProperties,
                                new DirectJoinRandomRoomResponse(roomToRandomJoin.GetRoomId()),
                                new DirectJoinRandomRoomResponse() {ResultCode = ResultCode.RequestProcessingError});
                        }
                            
                        break;
                    case ShamanOperationCode.JoinRoom:
                        var joinMessage = _serializer.DeserializeAs<JoinRoomRequest>(message.Buffer, message.Offset, message.Length);
                        
                        var roomToJoin = GetRoomById(joinMessage.RoomId);

                        if (roomToJoin == null)
                        {
                            var msg = $"Peer {sessionId} attempted to join to non-exist room {joinMessage.RoomId}";
                            _logger.Error(msg);
                            _messageSender.Send(new JoinRoomResponse() { ResultCode = ResultCode.RequestProcessingError }, peer);
                        }
                        else
                        {
                            JoinRoomAndReport(roomToJoin, sessionId, peer, joinMessage.Properties,
                                new JoinRoomResponse(),
                                new JoinRoomResponse() {ResultCode = ResultCode.RequestProcessingError});
                        }
                        
                        break;
                    case ShamanOperationCode.LeaveRoom:
                        PeerDisconnected(peer, new SimpleDisconnectInfo(ShamanDisconnectReason.PeerLeave));
                        break;
                    case ShamanOperationCode.Bundle:
                        if (_sessionsToRooms.TryGetValue(peer.GetSessionId(), out var room))
                            room.ProcessMessage(message, deliveryOptions, peer.GetSessionId());
                        else
                        {
                            _logger.Error($"ProcessMessage error: Can not get room for peer {peer.GetSessionId()}");
                            _messageSender.Send(new ErrorResponse() {ResultCode = ResultCode.MessageProcessingError}, peer);
                        }

                        break;
                    default:
                        throw new RoomManagerException($"Unknown operation code received: {operationCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"RoomManager.ProcessMessage: Error processing {operationCode} message: {ex}");
                _messageSender.Send(new ErrorResponse() {ResultCode = ResultCode.MessageProcessingError}, peer);
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