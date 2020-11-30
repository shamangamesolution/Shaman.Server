using System;
using System.Collections.Generic;

namespace Shaman.Contract.Bundle.Stats
{
    public class RegisteredServerStat
    {
        public string Address { get; set; }
        public DateTime? ActualizedOn { get; set; }      
    }
    
    public class MmServerStats
    {
        public List<RegisteredServerStat> RegisteredServers;
        public int TotalPlayers;
        public DateTime? OldestPlayerInMatchMaking;
        public int CreatedRoomsCount;
    }
}