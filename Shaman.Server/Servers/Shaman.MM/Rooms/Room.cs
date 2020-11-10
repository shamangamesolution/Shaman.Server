using System;
using System.Collections.Generic;
using Shaman.Messages.MM;

namespace Shaman.MM.Rooms
{
    public class Room
    {
        public Room(Guid id, int totalPlayersNeeded, int gameServerId, Dictionary<byte, object> properties, int currentPlayers, RoomState state)
        {
            Id = id;
            CreatedOn = DateTime.UtcNow;
            StateUpdatedOn = DateTime.UtcNow;
            ServerId = gameServerId;
            Properties = properties;
            TotalWeightNeeded = totalPlayersNeeded;
            CurrentWeight = currentPlayers;
            State = state;
            //set this to zero to wait update from game server
            MaxWeightToJoin = 0;
            // State = RoomState.Open;
        }
        
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime StateUpdatedOn { get; set; }
        public int TotalWeightNeeded { get; set; }
        public int CurrentWeight { get; set; }
        public int MaxWeightToJoin { get; set; }
        public int ServerId { get; set; }
        public Dictionary<byte, object> Properties { get; set; }
        
        public RoomState State { get; set; }
        
        // public void AddPlayers(int playersCount)
        // {
        //     CurrentPlayersCount += playersCount;
        // }
        public bool IsOpen()
        {
            return State == RoomState.Open; 
        }

        public bool CanJoin(int sumWeightInList, int maxWeightInList)
        {
            return IsOpen() && (((TotalWeightNeeded - CurrentWeight) >= sumWeightInList)) && MaxWeightToJoin >= maxWeightInList;
        }

        public void UpdateState(RoomState newState)
        {
            State = newState;
            StateUpdatedOn = DateTime.UtcNow;
        }
        
        public void Update(RoomState newState, int currentPlayersCount)
        {
            State = newState;
            StateUpdatedOn = DateTime.UtcNow;
            CurrentWeight = currentPlayersCount;
            //set this to zero to wait update from game server
            MaxWeightToJoin = 0;
        }
    }
}