using System;
using System.Collections.Generic;
using Shaman.MM.Peers;
using Shaman.Messages.RoomFlow;

namespace Shaman.MM.Players
{
    public class MatchMakingPlayer
    {
        public MmPeer Peer { get; set; }
        public Dictionary<byte, object> Properties { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? AddedToMmGroupOn { get; set; }
        public bool OnMatchmaking { get; set; }
        public bool MatchMakingComplete { get; set; }
        public JoinInfo JoinInfo { get; set; }

        private int _propertiesHashCode;
        
        public Guid Id
        {
            get { return Peer.GetPeerId(); }
        }

        public Guid SessionId
        {
            get { return Peer.GetSessionId(); }
        }

        public int PropertiesHashCode => _propertiesHashCode;
        
        public MatchMakingPlayer(MmPeer peer, Dictionary<byte, object> properties)
        {
            this.Peer = peer;
            this.Properties = properties;
            this.AddedToMmGroupOn = null;
            _propertiesHashCode = properties.GetHashCode();
        }
    }
}