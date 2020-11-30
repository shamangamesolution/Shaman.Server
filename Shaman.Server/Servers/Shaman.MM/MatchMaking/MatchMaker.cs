using System;
using System.Collections.Generic;
using Shaman.MM.Peers;
using Shaman.MM.Players;
using Shaman.MM.Managers;

namespace Shaman.MM.MatchMaking
{
    public class MatchMaker : IMatchMaker
    {
        private readonly List<byte> _requiredMatchMakingProperties;
        private readonly IPlayersManager _playersManager;
        private readonly IMatchMakingGroupsManager _groupManager;
        
        //hashcodes lists
        //private Dictionary<Guid, int> _hashCodeSets = new Dictionary<Guid, int>();

        public MatchMaker(IPlayersManager playersManager, IMatchMakingGroupsManager groupManager)
        {
            _playersManager = playersManager;
            _groupManager = groupManager;
            _requiredMatchMakingProperties = new List<byte>();
        }
        
        public void AddMatchMakerProperty(byte requiredMatchMakingProperty)
        {
            _requiredMatchMakingProperties.Add(requiredMatchMakingProperty);
        }

        public void AddPlayer(MmPeer peer, Dictionary<byte, object> properties, int mmmWeight)
        {
            var player = new MatchMakingPlayer(peer, properties, mmmWeight);
            _groupManager.AddPlayerToMatchMaking(player);
            // var groups = _groupManager.GetMatchmakingGroupIds(properties);
            // if (groups == null || groups.Count == 0)
            //     _logger.Error($"MatchMaker.AddPlayer error: no groups for player");
            // else
            //     _playersManager.Add(player, groups);
        }

        public void RemovePlayer(Guid peerId)
        {
            _playersManager.Remove(peerId);
        }

        public List<byte> GetRequiredProperties()
        {
            return _requiredMatchMakingProperties;
        }

        public void Clear()
        {
            _playersManager.Clear();
        }

        public void AddMatchMakingGroup(Dictionary<byte, object> measures)
        {
            //_groupManager.AddMatchMakingGroup(measures);
        }
        
        public void AddRequiredProperty(byte requiredMatchMakingProperty)
        {
            _requiredMatchMakingProperties.Add(requiredMatchMakingProperty);
        }

        public void Start()
        {
            _groupManager.Start();
        }

        public void Stop()
        {
            _groupManager.Stop();

            Clear();
        }
        
        
    }
}