using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Messages;
using Shaman.Messages.General.Entity;
using Shaman.Messages.MM;
using Shaman.MM.Providers;
using Shaman.MM.Rooms;
using Shaman.Routing.Common.MM;

namespace Shaman.MM.Managers
{
    public class RoomManager : IRoomManager
    {
        private readonly IMatchMakerServerInfoProvider _serverProvider;
        private readonly IShamanLogger _logger;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IRoomApiProvider _roomApiProvider;
        
        private readonly ConcurrentDictionary<Guid, Room> _rooms = new ConcurrentDictionary<Guid, Room>();
        private readonly ConcurrentDictionary<Guid, List<Room>> _groupToRoom = new ConcurrentDictionary<Guid, List<Room>>();
        private readonly ConcurrentDictionary<Guid, Guid> _roomToGroupId = new ConcurrentDictionary<Guid, Guid>();
        private readonly Queue<Room> _roomsQueue = new Queue<Room>();
        private IPendingTask _clearTask, _closeTask;
        
        private readonly object _roomQueueSync = new object();

        public RoomManager(IMatchMakerServerInfoProvider serverProvider, IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IRoomApiProvider roomApiProvider)
        {
            _serverProvider = serverProvider;
            _logger = logger;
            _roomApiProvider = roomApiProvider;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
        }

        public async Task<JoinRoomResult> CreateRoom(Guid groupId, Dictionary<Guid, Dictionary<byte, object>> players,
            Dictionary<byte, object> roomProperties)
        {
            //create room
            //var server = _serversCollection.GetLessLoadedServer();
            var server = _serverProvider.GetLessLoadedServer();
            if (server == null)
                return new JoinRoomResult {Result = RoomOperationResult.ServerNotFound};
            
            _logger.Info($"MmGroup: creating room on {server.Identity}");

            //prepare players dict to send to room
            var playersToSendToRoom = new Dictionary<Guid, Dictionary<byte, object>>();

            //move player from inner collection to new list
            foreach(var player in players)
            {
                //add additional property
                playersToSendToRoom.Add(player.Key, player.Value);
            }

            var room = RegisterRoom(groupId, roomProperties, Guid.NewGuid(), server, playersToSendToRoom);
            try
            {
                var url = UrlHelper.GetUrl(server.HttpPort, server.HttpsPort, server.Address);
                await _roomApiProvider.CreateRoom(url, room.Id, roomProperties, playersToSendToRoom);

                //add to queue
                lock (_roomQueueSync)
                {
                    _roomsQueue.Enqueue(room);
                }

                var port = server.GetLessLoadedPort();
                return new JoinRoomResult {Address = server.Address, Port = port, RoomId = room.Id, Result = RoomOperationResult.OK};
            }
            catch
            {
                DeregisterRoom(room.Id);
                return new JoinRoomResult {Result = RoomOperationResult.CreateRoomError};
            }
        }

        private Room RegisterRoom(Guid groupId, Dictionary<byte, object> roomProperties, Guid roomId, ServerInfo server,
            Dictionary<Guid, Dictionary<byte, object>> playersToSendToRoom)
        {
            //create room to allow joining during game
            if (!roomProperties.TryGetValue(PropertyCode.RoomProperties.TotalPlayersNeeded, out var totalPlayersProperty))
                throw new Exception($"MatchMakingGroup ctr error: there is no TotalPlayersNeeded property");

            var createdRoom = new Room(roomId, (int) totalPlayersProperty, server.Id, roomProperties);
            createdRoom.UpdateState(RoomState.Closed);
            createdRoom.AddPlayers(playersToSendToRoom.Count);

            //add to main collection
            _rooms.TryAdd(roomId, createdRoom);
            //add to group-to-room list
            if (!_groupToRoom.ContainsKey(groupId))
                _groupToRoom.TryAdd(groupId, new List<Room>());
            _groupToRoom[groupId].Add(createdRoom);
            
            //add to room-to-group
            _roomToGroupId.TryAdd(roomId, groupId);
            return createdRoom;
        }

        public async Task<JoinRoomResult> JoinRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            var room = _rooms[roomId];

            if (!room.CanJoin(players.Count))
                return new JoinRoomResult() {Result = RoomOperationResult.JoinRoomError};
            
            //update room with new players data
            var server = _serverProvider.GetServer(room.ServerId);
            if (server == null)
                return new JoinRoomResult() {Result = RoomOperationResult.ServerNotFound};

            try
            {
                var url = UrlHelper.GetUrl(server.HttpPort, server.HttpsPort, server.Address);
                await _roomApiProvider.UpdateRoom(url, players, roomId);
            
                var port = server.GetLessLoadedPort();
            
                //add new players to room
                room.AddPlayers(players.Count);
            
                return new JoinRoomResult {Address = server.Address, Port = port, RoomId = roomId, Result = RoomOperationResult.OK};
            }
            catch
            {
                return new JoinRoomResult() {Result = RoomOperationResult.JoinRoomError};
            }
        }

        public void UpdateRoomState(Guid roomId, int currentPlayers, RoomState roomState)
        {
            var room = GetRoom(roomId);
            if (room == null)
            {
                _logger.Error($"UpdateRoomState error: no room with id {roomId}");
                return;
            }

            room.CurrentPlayersCount = currentPlayers;
            room.UpdateState(roomState);
            _logger.Debug($"Update received: {roomId} State {roomState}");
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
        

        public void Start(int timeToKeepCreatedRoomSec, int timeIntervalToCloseOpenRoomsWithoutUpdatesMs = 5000)
        {
            //start created rooms clear task
            var checkPeriodMs = 60000; // 10 time
            _clearTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                var cnt = 0;
                lock (_roomQueueSync)
                {
                    while (_roomsQueue.TryPeek(out var room) && (DateTime.UtcNow - room.CreatedOn).TotalSeconds >
                           timeToKeepCreatedRoomSec)
                    {
                        var roomId = _roomsQueue.Dequeue().Id;
                        DeregisterRoom(roomId);
                        cnt++;
                    }
                }
                _logger.Info($"Cleaned {cnt} rooms");
            }, 0, checkPeriodMs);

            if (timeIntervalToCloseOpenRoomsWithoutUpdatesMs > 0)
            {
                _closeTask = _taskScheduler.ScheduleOnInterval(() =>
                {
                    foreach (var room in _rooms)
                    {
                        if (room.Value.IsOpen() && (DateTime.UtcNow - room.Value.StateUpdatedOn).TotalMilliseconds >
                            timeIntervalToCloseOpenRoomsWithoutUpdatesMs)
                            room.Value.UpdateState(RoomState.Closed);
                    }
                }, 0, 1000);
            }

        }

        private void DeregisterRoom(Guid roomId)
        {
            _rooms.TryRemove(roomId, out _);
            if (_roomToGroupId.TryRemove(roomId, out var groupId))
            {
                if (_groupToRoom.TryGetValue(groupId, out var group))
                {
                    @group.RemoveAll(r => r.Id == roomId);
                }
            }
        }

        public void Stop()
        {
            _taskScheduler.Remove(_clearTask);
            if (_closeTask != null)
                _taskScheduler.Remove(_closeTask);
            _rooms.Clear();
            _groupToRoom.Clear();
            lock (_roomQueueSync)
            {
                _roomsQueue.Clear();
            }
            _roomToGroupId.Clear();
        }
    }
}