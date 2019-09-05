using System;
using System.Collections.Generic;
using System.Net;

namespace Shaman.Messages.Stats
{
    public class RegisteredServerStat
    {
        public string Address { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime ActualizedOn { get; set; }      
    }
    
    public class MmServerStats
    {
        public List<RegisteredServerStat> RegisteredServers;
        public int TotalPlayers;
        public DateTime? OldestPlayerInMatchMaking;
    }
}