using System;
using System.Collections.Generic;

namespace Shaman.MM.MatchMaking
{
    public class CreatedRoom
    {
        public CreatedRoom(Guid id, int botsAdded, int closingInMs, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            Id = id;
            BotsAdded = botsAdded;
            ClosingInMs = closingInMs;
            CreatedOn = DateTime.UtcNow;
            Players = players;
        }

        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ClosingInMs { get; set; }
        public int BotsAdded { get; set; }
        public Dictionary<Guid, Dictionary<byte, object>> Players { get; set; }
    }
}