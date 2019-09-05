using System;
using System.Collections.Generic;
using Shaman.Common.Server.Peers;

namespace Shaman.Game.Rooms
{
    public class RoomPlayer
    {
        public IPeer Peer { get; set; }
        public DateTime? JoinedOn { get; set; }
        public Dictionary<byte, object> Properties { get; set; }

        public RoomPlayer(IPeer peer, Dictionary<byte, object> properties)
        {
            Peer = peer;
            Properties = properties;
            JoinedOn = DateTime.UtcNow;
        }
    }
}