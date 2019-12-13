using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Routing;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Players;
using Shaman.MM.Providers;
using Shaman.MM.Rooms;

namespace Shaman.MM.Managers
{
    public class RoomManager : IRoomManager
    {
        private readonly IMatchMakerServerInfoProvider _serverProvider;
        private readonly IShamanLogger _logger;
        private readonly ITaskScheduler _taskScheduler;
        
        private ConcurrentDictionary<Guid, Room> _rooms = new ConcurrentDictionary<Guid, Room>();
        private ConcurrentDictionary<Guid, List<Room>> _groupToRoom = new ConcurrentDictionary<Guid, List<Room>>();
        private ConcurrentDictionary<Guid, Guid> _roomToGroupId = new ConcurrentDictionary<Guid, Guid>();
        private ConcurrentDictionary<Guid, DateTime> _romIdToCloseTime = new ConcurrentDictionary<Guid, DateTime>();
        private Queue<Room> _roomsQueue = new Queue<Room>();
        private PendingTask _clearTask, _closeTask;

        public RoomManager(IMatchMakerServerInfoProvider serverProvider, IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory)
        {
            _serverProvider = serverProvider;
            _logger = logger;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
        }

        public JoinRoomResult CreateRoom(Guid groupId, Dictionary<Guid, Dictionary<byte, object>> players,
            Dictionary<byte, object> roomProperties)
        {
            //create room
            //var server = _serversCollection.GetLessLoadedServer();
            var server = _serverProvider.GetLessLoadedServer();
            if (server == null)
                return new JoinRoomResult {Result = RoomOperationResult.ServerNotFound};

            _logger.Info($"MmGroup: creating room on {server.Identity}");

            //prepare players dict to send to room
            var _playersToSendToRoom = new Dictionary<Guid, Dictionary<byte, object>>();

            var totalHumanPlayers = players.Count();
            //move player from inner collection to new list
            foreach(var player in players)
            {
                //add additional property
                _playersToSendToRoom.Add(player.Key, player.Value);
            }

            Guid roomId = _serverProvider.CreateRoom(server.Id, roomProperties, _playersToSendToRoom);
            if (roomId == Guid.Empty)
            {
                return new JoinRoomResult {Result = RoomOperationResult.CreateRoomError};
            }

            var port = server.GetLessLoadedPort();

            //create room to allow joining during game
            if (!roomProperties.TryGetValue(PropertyCode.RoomProperties.TotalPlayersNeeded, out var totalPlayersProperty))
                throw new Exception($"MatchMakingGroup ctr error: there is no TotalPlayersNeeded property");
            
            var createdRoom = new Room(roomId, (int)totalPlayersProperty, server.Id, roomProperties);
            createdRoom.UpdateState(RoomState.Closed);
            createdRoom.AddPlayers(_playersToSendToRoom.Count);
            
            //add to main collection
            _rooms.TryAdd(roomId, createdRoom);
            //add to group-to-room list
            if (!_groupToRoom.ContainsKey(groupId))
                _groupToRoom.TryAdd(groupId, new List<Room>());
            _groupToRoom[groupId].Add(createdRoom);
            //add to queue
            _roomsQueue.Enqueue(createdRoom);
            //add to room-to-group
            _roomToGroupId.TryAdd(roomId, groupId);


            return new JoinRoomResult {Address = server.Address, Port = port, RoomId = roomId, Result = RoomOperationResult.OK};
        }

        public JoinRoomResult JoinRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            var totalHumanPlayers = players.Count;

            var room = _rooms[roomId];

            if (!room.CanJoin(players.Count))
                return new JoinRoomResult() {Result = RoomOperationResult.JoinRoomError};
            
            //update room with new players data
            var server = _serverProvider.GetServer(room.ServerId);
            if (server == null)
                return new JoinRoomResult() {Result = RoomOperationResult.ServerNotFound};
            
            _serverProvider.UpdateRoom(room.ServerId, players, roomId);
            
            var port = server.GetLessLoadedPort();
            
            //add new players to room
            room.AddPlayers(players.Count);
            
            return new JoinRoomResult {Address = server.Address, Port = port, RoomId = roomId, Result = RoomOperationResult.OK};
        }

        public void UpdateRoomState(Guid roomId, int currentPlayers, int closingInMs, RoomState roomState)
        {
            var room = GetRoom(roomId);
            if (room == null)
            {
                _logger.Error($"UpdateRoomState error: no room with id {roomId}");
                return;
            }

            room.CurrentPlayersCount = currentPlayers;
            room.UpdateState(roomState);
            if (closingInMs > 0)
            {
                _romIdToCloseTime.TryRemove(roomId, out var prevRoom);
                _romIdToCloseTime.TryAdd(roomId, DateTime.UtcNow.AddMilliseconds(closingInMs));
            }
        }

        public Room GetRoom(Guid groupId, int playersCount)
        {
            if (!_groupToRoom.ContainsKey(groupId))
                return null;
            
            return _groupToRoom[groupId].FirstOrDefault(r => r.CanJoin(playersCount));
        }

        public Room GetRoom(Guid roomId)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
                return null;

            return room;
        }

        public IEnumerable<Room> GetAllRooms()
        {
            return _rooms.Select(l => l.Value).ToList();
        }

        public IEnumerable<Room> GetRooms(Guid groupId, bool openOnly = true, int limit = 10)
        {
            if (!_groupToRoom.ContainsKey(groupId))
                return new List<Room>();

            return _groupToRoom[groupId].Where(r => (r.IsOpen() && openOnly) || (!openOnly)).Take(limit);
        }

        public int GetRoomsCount()
        {
            return _rooms.Count;
        }
        

        public void Start(int timeToKeepCreatedRoomSec)
        {
            _rooms = new ConcurrentDictionary<Guid, Room>();
            _groupToRoom = new ConcurrentDictionary<Guid, List<Room>>();
            _roomToGroupId = new ConcurrentDictionary<Guid, Guid>();
            _roomsQueue = new Queue<Room>();
            
            //start created rooms close task
            _closeTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                var toCloseNow = _romIdToCloseTime.Where(c => c.Value <= DateTime.UtcNow).ToList();
                foreach (var roomId in toCloseNow)
                {
                    if (_rooms.TryGetValue(roomId.Key, out var room))
                        room.UpdateState(RoomState.Closed);
                    else
                        _romIdToCloseTime.TryRemove(roomId.Key, out var dt);
                }
            }, 0, 1000);
            
            //start created rooms clear task
            _clearTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                if (_roomsQueue.Count == 0)
                    return;
                
                var cnt = 0;
                while (_roomsQueue.Any() && (DateTime.UtcNow - _roomsQueue.First().CreatedOn).TotalSeconds >
                       timeToKeepCreatedRoomSec)
                {
                    var room = _roomsQueue.Dequeue();
                    _rooms.TryRemove(room.Id, out var room1);
                    if (_roomToGroupId.TryGetValue(room.Id, out var groupId))
                    {
                        if (_groupToRoom.TryGetValue(groupId, out var group))
                        {
                            group.RemoveAll(r => r.Id == room.Id);
                        }
                    }
                    cnt++;
                }
                
                _logger?.Info($"Cleaned {cnt} rooms");
            }, 0, timeToKeepCreatedRoomSec/2);
            
            
        }

        public void Stop()
        {
            _taskScheduler.Remove(_clearTask);
            _taskScheduler.Remove(_closeTask);
            _rooms.Clear();
            _groupToRoom.Clear();
            _roomsQueue.Clear();
            _roomToGroupId.Clear();
        }
    }
}