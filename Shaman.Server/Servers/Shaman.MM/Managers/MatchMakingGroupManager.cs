using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.Messages;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Players;
using Shaman.MM.Rooms;

namespace Shaman.MM.Managers
{
    public class MatchMakingGroupManager : IMatchMakingGroupsManager
    {

        private readonly IShamanLogger _logger;
        private readonly ITaskSchedulerFactory _taskSchedulerFactory;
        private readonly IPlayersManager _playersManager;
        private readonly IShamanMessageSender _messageSender;
        private readonly IMmMetrics _mmMetrics;
        private readonly IRoomManager _roomManager;
        private readonly IRoomPropertiesProvider _roomPropertiesProvider;
        private readonly IApplicationConfig _config;

        private readonly Dictionary<Guid, MatchMakingGroup> _groups = new Dictionary<Guid, MatchMakingGroup>();
        private readonly Dictionary<Guid, Dictionary<byte, object>> _groupsToProperties = new Dictionary<Guid, Dictionary<byte, object>>();
        private readonly object _mutex = new object();
        private bool _isStarted = false;
        
        public MatchMakingGroupManager(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory,
            IPlayersManager playersManager, IShamanMessageSender messageSender, IMmMetrics mmMetrics, IRoomManager roomManager, 
            IRoomPropertiesProvider roomPropertiesProvider, IApplicationConfig config)
        {
            _logger = logger;
            _taskSchedulerFactory = taskSchedulerFactory;
            _playersManager = playersManager;
            _messageSender = messageSender;
            _mmMetrics = mmMetrics;
            _roomManager = roomManager;
            _roomPropertiesProvider = roomPropertiesProvider;
            _config = config;
        }

        private bool AreDictionariesEqual(Dictionary<byte, object> dict1, Dictionary<byte, object> dict2)
        {
            if (dict1 == null && dict2 == null)
                return true;
            if (dict1 == null)
                return false;
            if (dict2 == null)
                return false;

            if (dict1.Count != dict2.Count)
                return false;

            foreach (var item in dict1)
            {
                if (!dict2.ContainsKey(item.Key) || !Equals(dict2[item.Key], item.Value))
                    return false;
            }

            return true;
        }
        
        public Guid AddMatchMakingGroup(Dictionary<byte, object> measures)
        {
            lock (_mutex)
            {
                var roomProperties = new Dictionary<byte, object>();
                roomProperties.Add(PropertyCode.RoomProperties.MatchMakingTick,
                    _roomPropertiesProvider.GetMatchMakingTick(measures));
                roomProperties.Add(PropertyCode.RoomProperties.MaximumMmTime,
                    _roomPropertiesProvider.GetMaximumMatchMakingTime(measures));
                roomProperties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded,
                    _roomPropertiesProvider.GetMaximumPlayers(measures));
                roomProperties.Add(PropertyCode.RoomProperties.MatchMakerUrl,
                    UrlHelper.GetUrl(_config.BindToPortHttp, 0, _config.PublicDomainNameOrAddress));

                foreach (var add in _roomPropertiesProvider.GetAdditionalRoomProperties(measures))
                    roomProperties.Add(add.Key, add.Value);

                var group = new MatchMakingGroup(roomProperties, _logger, _taskSchedulerFactory, _playersManager,
                    _messageSender, _mmMetrics, _roomManager);
                _groups.Add(group.Id, group);
                _groupsToProperties.Add(group.Id, measures);
                if (_isStarted)
                    group.Start();
                
                var str = new StringBuilder();
                str.Append($"New MM group added (total {_groups.Count}): properties: \n");
                foreach (var item in roomProperties)
                    str.Append($"[{item.Key}]: {item.Value} \n");
                
                _logger.Error(str.ToString());
                
                return group.Id;
            }
        }

        public List<Guid> GetMatchmakingGroupIds(Dictionary<byte, object> playerProperties)
        {
            lock (_mutex)
            {
                var result = new List<Guid>();
                foreach (var group in _groupsToProperties)
                    if (AreDictionariesEqual(group.Value, playerProperties))
                        result.Add(group.Key);
                return result;
            }
        }

        public Dictionary<byte, object> GetRoomProperties(Guid groupId)
        {
            lock (_mutex)
            {
                if (!_groups.TryGetValue(groupId, out var group))
                    throw new Exception($"GetRoomProperties error: there is no MmGroup {groupId}");

                return group.RoomProperties;
            }
        }

        public IEnumerable<Room> GetRooms(Dictionary<byte, object> playerProperties)
        {
            lock (_mutex)
            {
                var groups = GetMatchmakingGroupIds(playerProperties);
                var rooms = new List<Room>();
                foreach (var group in groups)
                    rooms.AddRange(
                        _roomManager
                            .GetRooms(group)
                    );
                return rooms;
            }
        }

        public void AddPlayerToMatchMaking(MatchMakingPlayer player)
        {
            lock (_mutex)
            {
                var groups = GetMatchmakingGroupIds(player.Properties);
                if (groups == null || groups.Count == 0)
                {
                    _logger.Info($"AddPlayerToMatchMaking error: no groups for player");
                    groups = new List<Guid>();
                    groups.Add(AddMatchMakingGroup(player.Properties));
                }

                _playersManager.Add(player, groups);
            }
        }

        public async Task<JoinRoomResult> CreateRoom(Guid sessionId, Dictionary<byte, object> playerProperties)
        {
            Guid groupId;
            lock (_mutex)
            {
                //get supported group IDs
                var groupIds =
                    GetMatchmakingGroupIds(playerProperties);
                if (groupIds == null || groupIds.Count == 0)
                {
                    //TODO auto create group
                    //throw new Exception($"CreateRoomFromClient error: no group for requested properties is not exists");
                    groupIds = new List<Guid>();
                    groupIds.Add(AddMatchMakingGroup(playerProperties));
                }

                groupId = groupIds[0];
            }

            //create room in chosen group
            return await _roomManager.CreateRoom(groupId, new Dictionary<Guid, Dictionary<byte, object>> {{sessionId, playerProperties}}, GetRoomProperties(groupId));
        }

        public void Start(int timeToKeepCreatedRoomSec = 1800, int timeIntervalToCloseOpenRoomsWithoutUpdatesMs = 5000)
        {
            foreach(var group in _groups)
                group.Value.Start();
            
            _roomManager.Start(timeToKeepCreatedRoomSec, timeIntervalToCloseOpenRoomsWithoutUpdatesMs);

            _isStarted = true;
        }

        public void Stop()
        {
            foreach(var group in _groups)
                group.Value.Stop();
            _groups.Clear();
            _groupsToProperties.Clear();
            _roomManager.Stop();
            _isStarted = false;
        }
    }
}