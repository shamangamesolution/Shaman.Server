using System;
using System.Collections.Generic;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.MM.MatchMaking
{
    public class CreatedRoom
    {
        public CreatedRoom(Guid id, int botsAdded, int closingInMs, Dictionary<Guid, Dictionary<byte, object>> players, ServerInfo server, bool addOtherPlayers)
        {
            Id = id;
            BotsAdded = botsAdded;
            ClosingInMs = closingInMs;
            CreatedOn = DateTime.UtcNow;
            Players = players;
            Server = server;
            AddOtherPlayers = addOtherPlayers;
        }

        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ClosingInMs { get; set; }
        public int BotsAdded { get; set; }
        public Dictionary<Guid, Dictionary<byte, object>> Players { get; set; }
        //public RegisteredServer Server { get; set; }
        public ServerInfo Server { get; set; }
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