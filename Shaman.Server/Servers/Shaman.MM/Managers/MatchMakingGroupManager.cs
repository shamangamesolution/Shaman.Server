using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;

namespace Shaman.MM.Managers
{
    public class MatchMakingGroupManager : IMatchMakingGroupsManager
    {

        private readonly IShamanLogger _logger;
        private readonly ITaskSchedulerFactory _taskSchedulerFactory;
        private readonly IPlayersManager _playersManager;
        private readonly IPacketSender _packetSender;
        private readonly IMmMetrics _mmMetrics;
        private readonly IRoomManager _roomManager;
        private readonly IMatchMakerServerInfoProvider _serverInfoProvider;
        private readonly IBotManager _botManager;
        
        private Dictionary<Guid, MatchMakingGroup> _groups = new Dictionary<Guid, MatchMakingGroup>();
        private Dictionary<Guid, Dictionary<byte, object>> _groupsToProperties = new Dictionary<Guid, Dictionary<byte, object>>();
        private bool _isStarted = false;
        
        public MatchMakingGroupManager(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory,
            IPlayersManager playersManager, IPacketSender packetSender, IMmMetrics mmMetrics,
            IMatchMakerServerInfoProvider serverInfoProvider, IRoomManager roomManager, IBotManager botManager)
        {
            _logger = logger;
            _taskSchedulerFactory = taskSchedulerFactory;
            _playersManager = playersManager;
            _packetSender = packetSender;
            _mmMetrics = mmMetrics;
            _serverInfoProvider = serverInfoProvider;
            _roomManager = roomManager;
            _botManager = botManager;
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
                if (!dict2.ContainsKey(item.Key) || Convert.ToInt32(dict2[item.Key]) != Convert.ToInt32(item.Value))
                    return false;
            }

            return true;
        }
        
        public Guid AddMatchMakingGroup(Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures)
        {
            var group = new MatchMakingGroup(roomProperties, measures, _logger, _taskSchedulerFactory, _playersManager, _packetSender, _mmMetrics, _roomManager, _botManager);
            _groups.Add(group.Id, group);
            _groupsToProperties.Add(group.Id, measures);
            if (_isStarted)
                group.Start();
            return group.Id;
        }

        public List<Guid> GetMatchmakingGroupIds(Dictionary<byte, object> playerProperties)
        {
            var result = new List<Guid>();
            foreach(var group in _groupsToProperties)
                if (AreDictionariesEqual(group.Value, playerProperties))
                    result.Add(group.Key);
            return result;
        }

        public void Start(int timeToKeepCreatedRoomSec = 1800)
        {
            foreach(var group in _groups)
                group.Value.Start();
            
            _roomManager.Start(timeToKeepCreatedRoomSec);

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