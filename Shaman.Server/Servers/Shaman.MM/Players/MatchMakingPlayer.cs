using System;
using System.Collections.Generic;
using Shaman.Common.Server.Peers;

namespace Shaman.MM.Players
{
    public class MatchMakingPlayer
    {
        public IPeer Peer { get; set; }
        public Dictionary<byte, object> Properties { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? AddedToMmGroupOn { get; set; }
        public bool OnMatchmaking { get; set; }
        public int MmWeight { get; set; }
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
        
        public MatchMakingPlayer(IPeer peer, Dictionary<byte, object> properties, int mmWeight = 1)
        {
            this.Peer = peer;
            this.Properties = properties;
            this.AddedToMmGroupOn = null;
            this.MmWeight = mmWeight;
            _propertiesHashCode = properties.GetHashCode();
        }
    }
}