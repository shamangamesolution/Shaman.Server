using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Servers;

namespace Shaman.MM.Rooms
{
    public class Room
    {
        public Room(Guid id, int botsAdded, int closingInMs, Dictionary<Guid, Dictionary<byte, object>> players, int serverId, bool addOtherPlayers, Dictionary<byte, object> properties, Dictionary<byte, object> measures)
        {
            Id = id;
            BotsAdded = botsAdded;
            ClosingInMs = closingInMs;
            CreatedOn = DateTime.UtcNow;
            Players = players;
            ServerId = serverId;
            AddOtherPlayers = addOtherPlayers;
            Properties = properties;
            Measures = measures;
        }

        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ClosingInMs { get; set; }
        public int BotsAdded { get; set; }
        public Dictionary<Guid, Dictionary<byte, object>> Players { get; set; }
        public int ServerId { get; set; }
        public Dictionary<byte, object> Properties { get; set; }
        public Dictionary<byte, object> Measures { get; set; }
        public bool AddOtherPlayers { get; set; }
        
        public void AddPlayers(Dictionary<Guid, Dictionary<byte, object>> players)
        {
            foreach (var player in players)
            {
                Players.Add(player.Key, player.Value);
            }

            BotsAdded -= players.Count;

            //dirty hack
            if (BotsAdded < 0)
                BotsAdded = 0;
        }
        public bool IsOpen()
        {
            return (DateTime.UtcNow - CreatedOn).TotalMilliseconds < ClosingInMs;
        }

        public bool CanJoin(int playersCount)
        {
            return IsOpen() && BotsAdded >= playersCount && AddOtherPlayers;
        }
    }
}