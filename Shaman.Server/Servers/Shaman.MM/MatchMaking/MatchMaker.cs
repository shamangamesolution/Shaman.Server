using System;
using System.Collections.Generic;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.MM.Peers;
using Shaman.MM.Players;
using Shaman.MM.Servers;
using Shaman.Messages.RoomFlow;

namespace Shaman.MM.MatchMaking
{
    public class MatchMaker : IMatchMaker
    {
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private IRegisteredServerCollection _serverCollection;
        private IPlayerCollection _playerCollection;
        private ISerializerFactory _serializerFactory;
        private List<MatchMakingGroup> _matchMakingGroups;
        private IShamanLogger _logger;
        private object _syncCollection = new object();
        private List<byte> _requiredMatchMakingProperties = new List<byte>();

        private IPacketSender _packetSender;
        //hashcodes lists
        //private Dictionary<Guid, int> _hashCodeSets = new Dictionary<Guid, int>();

        public MatchMaker(IRegisteredServerCollection serverCollection, IPlayerCollection playerCollection, IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, ISerializerFactory serializerFactory, IPacketSender packetSender)
        {
            _taskSchedulerFactory = taskSchedulerFactory;
            _serializerFactory = serializerFactory;
            _packetSender = packetSender;
            _serverCollection = serverCollection;
            _playerCollection = playerCollection;
            _logger = logger;
            _matchMakingGroups = new List<MatchMakingGroup>();
        }
        
        public void Initialize(List<byte> requiredMatchMakingProperties)
        {
            _requiredMatchMakingProperties = requiredMatchMakingProperties;
            
            //_hashCodeSets = new Dictionary<Guid, int>();
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

        public void AddMatchMakingGroup(int totalPlayersNeeded, int matchMakingTickMs, bool addBots, bool addOtherPlayers, int timeBeforeBotsAddedMs, Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures)
        {
            var mmGroup = new MatchMakingGroup(totalPlayersNeeded, matchMakingTickMs,addBots, addOtherPlayers, timeBeforeBotsAddedMs, roomProperties, measures, _logger, _taskSchedulerFactory, this, _serverCollection, _playerCollection, _serializerFactory, _packetSender);
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

        }
        
        
    }
}