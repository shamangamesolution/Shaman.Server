using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.MM.Peers;
using Shaman.MM.Players;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;

namespace Shaman.MM.MatchMaking
{
    public class MatchMaker : IMatchMaker
    {
        private readonly ICreatedRoomManager _createdRoomManager;
        private readonly ITaskSchedulerFactory _taskSchedulerFactory;
        private readonly IPlayerCollection _playerCollection;
        private readonly ISerializer _serializer;
        private readonly List<MatchMakingGroup> _matchMakingGroups;
        private readonly IShamanLogger _logger;
        private readonly IMatchMakerServerInfoProvider _serverProvider;
        private readonly List<byte> _requiredMatchMakingProperties;

        private readonly IPacketSender _packetSender;

        private readonly IMmMetrics _mmMetrics;
        //hashcodes lists
        //private Dictionary<Guid, int> _hashCodeSets = new Dictionary<Guid, int>();

        public MatchMaker(IPlayerCollection playerCollection,
            IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, ISerializer serializer,
            IPacketSender packetSender, IMmMetrics mmMetrics, ICreatedRoomManager createdRoomManager, IMatchMakerServerInfoProvider serverProvider)
        {
            _taskSchedulerFactory = taskSchedulerFactory;
            _serializer = serializer;
            _packetSender = packetSender;
            _mmMetrics = mmMetrics;
            _createdRoomManager = createdRoomManager;
            _serverProvider = serverProvider;
            _playerCollection = playerCollection;
            _logger = logger;
            _matchMakingGroups = new List<MatchMakingGroup>();
            _requiredMatchMakingProperties = new List<byte>();
        }
        
        public void AddMatchMakerProperty(byte requiredMatchMakingProperty)
        {
            _requiredMatchMakingProperties.Add(requiredMatchMakingProperty);
        }

        public void AddPlayer(MmPeer peer, Dictionary<byte, object> properties)
        {
            _playerCollection.Add(new MatchMakingPlayer(peer, properties));
        }

        public void RemovePlayer(Guid peerId)
        {
            _playerCollection.Remove(peerId);
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

        public void AddMatchMakingGroup(int totalPlayersNeeded, int matchMakingTickMs, bool addBots, bool addOtherPlayers, int timeBeforeBotsAddedMs, int roomClosingIn, Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures)
        {
            var mmGroup = new MatchMakingGroup(totalPlayersNeeded, matchMakingTickMs, addBots, addOtherPlayers,
                timeBeforeBotsAddedMs, roomClosingIn, roomProperties, measures, _logger, _taskSchedulerFactory, this,
                _playerCollection, _serializer, _packetSender, _mmMetrics, _createdRoomManager, _serverProvider);
            _matchMakingGroups.Add(mmGroup);
//            var hashCode = measures.GetHashCode();
//            _hashCodeSets.Add(mmGroup.Id, hashCode);
        }

        
        public void Start()
        {
            //start groups
            foreach (var group in _matchMakingGroups)
            {
                group.Start();
                _playerCollection.AddMmGroup(group.Id, group.Measures);
            }
            
            _createdRoomManager.Start();
        }

        public void Stop()
        {
            foreach (var group in _matchMakingGroups)
            {
                group.Stop();
            }
            
            _createdRoomManager.Stop();
            Clear();
        }
        
        
    }
}