using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Servers;
using Shaman.Messages;

namespace Shaman.MM.Rooms
{
    public class Room
    {
        public Room(Guid id, int totalPlayersNeeded,  int botsAdded, int closingInMs, Dictionary<Guid, Dictionary<byte, object>> players, int serverId, bool addOtherPlayers, Dictionary<byte, object> properties, Dictionary<byte, object> measures)
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
            TotalPlayersNeeded = totalPlayersNeeded;
        }

        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ClosingInMs { get; set; }
        public int TotalPlayersNeeded { get; set; }
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

            //remove bots
            Players
                .Where(p =>
                            p.Value.ContainsKey(PropertyCode.PlayerProperties.IsBot) &&
                            (bool) p.Value[PropertyCode.PlayerProperties.IsBot] == true)
                .Select(p => p.Key)
                .Take(players.Count)
                .ToList()
                .ForEach(item => { Players.Remove(item); });
            
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
            return IsOpen() && (BotsAdded >= playersCount || ((TotalPlayersNeeded - (Players.Count - BotsAdded)) >= playersCount)) && AddOtherPlayers;
        }
    }
}