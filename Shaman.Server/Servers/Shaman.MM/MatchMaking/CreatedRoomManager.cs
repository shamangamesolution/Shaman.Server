using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.MM.MatchMaking
{
    public class CreatedRoomManager : ICreatedRoomManager
    {
        private const int TIME_TO_KEEP_CREATED_ROOM_SEC = 1800;

        private readonly ITaskScheduler _taskScheduler;
        private readonly IShamanLogger _logger;
        private Queue<CreatedRoom> _createdRooms;
        private PendingTask _clearTask;
        private object _syncRooms = new object();
        
        public CreatedRoomManager(ITaskSchedulerFactory taskSchedulerFactory, IShamanLogger logger)
        {
            //deps
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _logger = logger;
        }
        
        public void AddCreatedRoom(CreatedRoom createdRoom)
        {
            lock (_syncRooms)
            {
                _createdRooms.Enqueue(createdRoom);
            }
        }

        public CreatedRoom GetRoomForPlayers(int playersCount)
        {
            lock (_syncRooms)
            {
                return _createdRooms.FirstOrDefault(r => r.CanJoin(playersCount));
            }
        }

        public int GetCreatedRoomsCount()
        {
            lock (_syncRooms)
            {
                return _createdRooms.Count;
            }
        }

        public void Start()
        {
            _createdRooms = new Queue<CreatedRoom>();
            
            //start created rooms clear task
            _clearTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                if (_createdRooms.Count == 0)
                    return;
                var cnt = 0;
                while (_createdRooms.Any() && (DateTime.UtcNow - _createdRooms.First().CreatedOn).TotalSeconds >
                       TIME_TO_KEEP_CREATED_ROOM_SEC)
                {
                    _createdRooms.Dequeue();
                    cnt++;
                }
                
                _logger?.Info($"Cleaned {cnt} rooms");
            }, 10000, 10000);
        }

        public void Stop()
        {
            _taskScheduler.RemoveAll();
            _createdRooms.Clear();
        }
    }
}