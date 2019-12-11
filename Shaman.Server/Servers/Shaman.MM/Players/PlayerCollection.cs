using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Metrics;

namespace Shaman.MM.Players
{
    public class PlayerCollection : IPlayerCollection
    {
        private object _syncCollection = new object();
        //private List<MatchMakingPlayer> _players = new List<MatchMakingPlayer>();
        //private Dictionary<byte, Dictionary<Guid, int>> _propertiesValues = new Dictionary<byte, Dictionary<Guid, int>>();
        private IShamanLogger _logger;
        
        private Dictionary<Guid, MatchMakingPlayer> _players;
        private Dictionary<Guid, Dictionary<byte, object>> _mmGroups = new Dictionary<Guid, Dictionary<byte, object>>();
        private Dictionary<Guid, List<MatchMakingPlayer>> _mmGroupToPlayer;
        private Dictionary<Guid, Guid> _playerToMmGroup = new Dictionary<Guid, Guid>();
        private readonly IMmMetrics _mmMetrics;

        public PlayerCollection(IShamanLogger logger, IMmMetrics mmMetrics)
        {
            _logger = logger;
            _mmMetrics = mmMetrics;
            _players = new Dictionary<Guid, MatchMakingPlayer>();
            _mmGroupToPlayer = new Dictionary<Guid, List<MatchMakingPlayer>>();
            _mmGroups = new Dictionary<Guid, Dictionary<byte, object>>();
            _playerToMmGroup = new Dictionary<Guid, Guid>();
        }

        public void AddMmGroup(Guid id, Dictionary<byte, object> properties)
        {
            if (_mmGroups == null)
                _mmGroups = new Dictionary<Guid, Dictionary<byte, object>>();
            
            _mmGroups.Add(id, properties);
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
        
        public void Add(MatchMakingPlayer player)
        {
            lock (_syncCollection)
            {
                _players.Add(player.Id, player);
                _mmMetrics.TrackPlayerAdded();

                foreach (var group in _mmGroups)
                {
                    if (AreDictionariesEqual(group.Value, player.Properties))
                    {
                        if (!_mmGroupToPlayer.ContainsKey(group.Key))
                            _mmGroupToPlayer.Add(group.Key, new List<MatchMakingPlayer>());
                        
                        _mmGroupToPlayer[group.Key].Add(player);
                        
                        _playerToMmGroup.Add(player.Id, group.Key); 
                    }
                }
            }    
        }

        public IEnumerable<MatchMakingPlayer> GetPlayersAndSetOnMatchmaking(Guid groupId, int maxCount)
        {
            lock (_syncCollection)
            {
                if (maxCount <= 0)
                    return new List<MatchMakingPlayer>();
                
                if (!_mmGroupToPlayer.ContainsKey(groupId))
                    return new List<MatchMakingPlayer>();
//                
                var playersNeeded = _mmGroupToPlayer[groupId].Where(p => p.OnMatchmaking == false).OrderBy(p => p.StartedOn).Take(maxCount).ToList();
                foreach (var player in playersNeeded)
                {
                    player.OnMatchmaking = true;
                }
                
                return playersNeeded;
            }
        }

//        public List<MatchMakingPlayer> GetPlayersByMeasureAndSetOnMatchMakingFlag(List<MatchMakingMeasure> measures, int totalNeeded)
//        {
//            var result = new List<MatchMakingPlayer>();
//
//            lock (_syncCollection)
//            {
//    
//                foreach (var player in _players.Where(p => !p.OnMatchmaking && !p.MatchMakingComplete))
//                {
//                    bool exitLoop = false;
//                    foreach (var measure in measures)
//                    {
//                        //if dict has no such property
//                        if (!_propertiesValues.ContainsKey(measure.PropertyCode))
//                        {
//                            _logger.Error($"PropertyDict has no property {measure.PropertyCode}");
//                            break;
//                        }
//    
//                        var propertyValue = _propertiesValues[measure.PropertyCode][player.Id];
//    
//                        switch (measure.MeasureOperation)
//                        {
//                            case MeasureOperation.Equal:
//                                if (propertyValue != measure.ExactValue)
//                                    exitLoop = true;
//                                break;
//                            case MeasureOperation.MinMax:
//                                if (propertyValue < measure.MinValueInclusive ||
//                                    propertyValue >= measure.MaxValueExclusive)
//                                    exitLoop = true;
//                                break;
//                            default:
//                                throw new ArgumentOutOfRangeException("MeasureOperation");
//                        }
//
//                        if (exitLoop) break;
//                        
//                        //if we get here - we can take this player to matchmaking
//                        player.OnMatchmaking = true;
//                        
//                        result.Add(player);
//                    }
//    
//                }
//            }
//            
//            return result;
//        }

        public void Remove(Guid peerId)
        {
            lock (_syncCollection)
            {
                if (!_players.ContainsKey(peerId))
                {
                    _logger.Info($"Matchmaker.PlayerCollection error: Trying to delete non-existing player");
                    return;
                }
                
                var player = _players[peerId];

                if (_playerToMmGroup.ContainsKey(peerId))
                {

                    var groupId = _playerToMmGroup[peerId];
                    if (!_mmGroupToPlayer.ContainsKey(groupId))
                    {
                        _logger.Error(
                            $"Matchmaker.PlayerCollection error: _propertyHashToPlayer dict has no {player.PropertiesHashCode} propertyHash");
                    }
                    else
                    {
                        _mmGroupToPlayer[groupId].RemoveAll(p => p.Id == peerId);
                    }

                    _playerToMmGroup.Remove(peerId);
                }

                _players.Remove(peerId);
                _mmMetrics.TrackPlayerRemoved();
            }
        }

        public MatchMakingPlayer GetPlayer(Guid peerId)
        {
            lock (_syncCollection)
            {
                if (!_players.ContainsKey(peerId))
                    return null;

                
                return _players[peerId];
            }
        }

        public MatchMakingPlayer GetOldestPlayer()
        {
            lock (_syncCollection)
            {
                if (_players.Count == 0)
                    return null;

                return _players.OrderBy(p => p.Value.StartedOn).FirstOrDefault().Value;
            }
        }

        public void Clear()
        {
            lock (_syncCollection)
            {
                var playersCount = _players.Count;
                _players.Clear();
                _mmGroupToPlayer.Clear();
                _mmMetrics.TrackPlayerCleared(playersCount);
            }
        }

        public int Count()
        {
            lock (_syncCollection)
            {
                return _players.Count();
            }
        }

        public void SetOnMatchmaking(Guid playerId, bool isOnMatchmaking)
        {
            var player = GetPlayer(playerId);
            lock (_syncCollection)
            {
                if (player != null)
                    player.OnMatchmaking = isOnMatchmaking;
            }
        }


    }
}