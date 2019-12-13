using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Servers;
using Shaman.Messages;
using Shaman.Messages.MM;

namespace Shaman.MM.Rooms
{
    public class Room
    {
        public Room(Guid id, int totalPlayersNeeded, int gameServerId, Dictionary<byte, object> properties)
        {
            Id = id;
            CreatedOn = DateTime.UtcNow;
            ServerId = gameServerId;
            Properties = properties;
            TotalPlayersNeeded = totalPlayersNeeded;
            State = RoomState.Open;
        }
        
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public int TotalPlayersNeeded { get; set; }
        public int CurrentPlayersCount { get; set; }
        public int ServerId { get; set; }
        public Dictionary<byte, object> Properties { get; set; }
        
        public RoomState State { get; set; }
        
        public void AddPlayers(int playersCount)
        {
            CurrentPlayersCount += playersCount;
        }
        public bool IsOpen()
        {
            return State == RoomState.Open; 
        }

        public bool CanJoin(int playersCount)
        {
            return IsOpen() && (((TotalPlayersNeeded - CurrentPlayersCount) >= playersCount));
        }

        public void UpdateState(RoomState newState)
        {
            State = newState;
        }
    }
}