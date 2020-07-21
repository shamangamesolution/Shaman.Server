using System;
using System.Collections.Generic;

namespace Shaman.Contract.Bundle.Stats
{
    public class GameServerStats 
    {
        public int RoomCount { get; set; }
        public int PeerCount { get; set; }
        public DateTime? OldestRoomCreatedOn { get; set; }        
        public Dictionary<Guid, int> RoomsPeerCount { get; set; }
        public Dictionary<ushort, int> PeersCountPerPort { get; set; }
    }
}