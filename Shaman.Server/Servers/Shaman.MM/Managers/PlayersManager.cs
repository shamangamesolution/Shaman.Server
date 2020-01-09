using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Metrics;
using Shaman.MM.Players;

namespace Shaman.MM.Managers
{
    public class PlayersManager : IPlayersManager
    {
        private readonly IMmMetrics _mmMetrics;
        private readonly IShamanLogger _logger;
        
        private object _syncCollection = new object();
        private Dictionary<Guid, MatchMakingPlayer> _players = new Dictionary<Guid, MatchMakingPlayer>();
        private Dictionary<Guid, List<MatchMakingPlayer>> _mmGroupToPlayer = new Dictionary<Guid, List<MatchMakingPlayer>>();
        private Dictionary<Guid, List<Guid>> _playerToMmGroup = new Dictionary<Guid, List<Guid>>();

        public PlayersManager(IMmMetrics mmMetrics, IShamanLogger logger)
        {
            _mmMetrics = mmMetrics;
            _logger = logger;
        }

        public void Add(MatchMakingPlayer player, List<Guid> groups)
        {
            lock (_syncCollection)
            {
                if (_players.ContainsKey(player.Id))
                    throw new Exception($"Player {player.Id} already added");
                
                _players.Add(player.Id, player);
                _mmMetrics.TrackPlayerAdded();

                foreach (var group in groups)
                {
                    if (!_mmGroupToPlayer.ContainsKey(group))
                        _mmGroupToPlayer.Add(group, new List<MatchMakingPlayer>());
                    
                    _mmGroupToPlayer[group].Add(player);
                    
                    if (!_playerToMmGroup.ContainsKey(player.Id))
                        _playerToMmGroup.Add(player.Id, new List<Guid>());
                    
                    _playerToMmGroup[player.Id].Add(group);
                }
            }   
        }

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

                    var groupIds = _playerToMmGroup[peerId];
                    foreach (var groupId in groupIds)
                    {
                        if (_mmGroupToPlayer.ContainsKey(groupId))
                        {
                            _mmGroupToPlayer[groupId].RemoveAll(p => p.Id == peerId);
                        }
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

        public void SetOnMatchmaking(Guid playerId, bool isOnMatchmaking)
        {
            var player = GetPlayer(playerId);
            lock (_syncCollection)
            {
                if (player != null)
                    player.OnMatchmaking = isOnMatchmaking;
            }
        }

        public void Clear()
        {
            lock (_syncCollection)
            {
                var playersCount = _players.Count;
                _players.Clear();
                _mmGroupToPlayer.Clear();
                _playerToMmGroup.Clear();
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

        public IEnumerable<MatchMakingPlayer> GetPlayers(Guid groupId, int maxCount)
        {
            lock (_syncCollection)
            {
                if (maxCount <= 0)
                    return new List<MatchMakingPlayer>();
                
                if (!_mmGroupToPlayer.ContainsKey(groupId))
                    return new List<MatchMakingPlayer>();

                return _mmGroupToPlayer[groupId].Where(p => p.OnMatchmaking == false).OrderBy(p => p.StartedOn).Take(maxCount).ToList();
            }
        }
    }
}