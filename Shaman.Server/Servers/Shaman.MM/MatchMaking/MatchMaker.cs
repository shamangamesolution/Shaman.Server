using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.MM.Peers;
using Shaman.MM.Players;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;

namespace Shaman.MM.MatchMaking
{
    public class MatchMaker : IMatchMaker
    {
        private readonly ICreatedRoomManager _createdRoomManager;
        private readonly IPlayerCollection _playerCollection;
        private readonly IShamanLogger _logger;
        private readonly List<byte> _requiredMatchMakingProperties;
        private readonly IPlayersManager _playersManager;
        private readonly IMatchMakingGroupsManager _groupManager;
        
        private readonly IPacketSender _packetSender;

        private readonly IMmMetrics _mmMetrics;
        //hashcodes lists
        //private Dictionary<Guid, int> _hashCodeSets = new Dictionary<Guid, int>();

        public MatchMaker(IPlayerCollection playerCollection,
            IShamanLogger logger,
            IPacketSender packetSender, IMmMetrics mmMetrics, ICreatedRoomManager createdRoomManager, IPlayersManager playersManager, IMatchMakingGroupsManager groupManager)
        {
            _packetSender = packetSender;
            _mmMetrics = mmMetrics;
            _createdRoomManager = createdRoomManager;
            _playersManager = playersManager;
            _groupManager = groupManager;
            _playerCollection = playerCollection;
            _logger = logger;
            _requiredMatchMakingProperties = new List<byte>();
        }
        
        public void AddMatchMakerProperty(byte requiredMatchMakingProperty)
        {
            _requiredMatchMakingProperties.Add(requiredMatchMakingProperty);
        }

        public void AddPlayer(MmPeer peer, Dictionary<byte, object> properties)
        {
            var player = new MatchMakingPlayer(peer, properties);
            _playerCollection.Add(player);
            var groups = _groupManager.GetMatchmakingGroupIds(properties);
            if (groups == null || groups.Count == 0)
                throw new Exception($"MatchMaker.AddPlayer error: no groups for player");
            _playersManager.Add(player, groups);
        }

        public void RemovePlayer(Guid peerId)
        {
            _playerCollection.Remove(peerId);
            _playersManager.Remove(peerId);
        }

        public JoinInfo GetJoinInfo(Guid peerId)
        {
            var player = _playerCollection.GetPlayer(peerId);
            if (player == null)
            {
                _logger.Error($"GetJoinInfo error: there is no player in collection");
                return null;
            }

            return player.JoinInfo;
        }

        public List<byte> GetRequiredProperties()
        {
            return _requiredMatchMakingProperties;
        }


        public void Clear()
        {
            _playerCollection.Clear();
        }

        public void AddMatchMakingGroup(Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures)
        {
            _groupManager.AddMatchMakingGroup(roomProperties, measures);
        }
        
        public void AddMatchMakingGroup(int totalPlayersNeeded, int matchMakingTickMs, bool addBots, bool addOtherPlayers, int timeBeforeBotsAddedMs, int roomClosingIn, Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures)
        {
//            var mmGroup = new MatchMakingGroup(totalPlayersNeeded, matchMakingTickMs, addBots, addOtherPlayers,
//                timeBeforeBotsAddedMs, roomClosingIn, roomProperties, measures, _logger, _taskSchedulerFactory, this,
//                _playerCollection, _serializer, _packetSender, _mmMetrics, _createdRoomManager, _serverProvider);
//            _matchMakingGroups.Add(mmGroup);
            
            
//            var hashCode = measures.GetHashCode();
//            _hashCodeSets.Add(mmGroup.Id, hashCode);
        }

        public void AddRequiredProperty(byte requiredMatchMakingProperty)
        {
            _requiredMatchMakingProperties.Add(requiredMatchMakingProperty);
        }

        public void Start()
        {
            _createdRoomManager.Start();
            
            _groupManager.Start();
        }

        public void Stop()
        {
            _createdRoomManager.Stop();
            _groupManager.Stop();

            Clear();
        }
        
        
    }
}