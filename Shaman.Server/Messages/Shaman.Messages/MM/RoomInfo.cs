using System;
using System.Collections.Generic;
using Shaman.Messages.Helpers;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.Messages.MM
{
    public enum RoomState : byte
    {
        Open,
        Closed,
        Disposed
    }
    
    public class RoomInfo : EntityBase
    {
        public Guid RoomId { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public RoomState State { get; set; }
        public Dictionary<byte, object> RoomProperties { get; set; }
        
        public RoomInfo(Guid roomId, int maxPlayers, int currentPlayers, Dictionary<byte, object> roomProperties, RoomState state)
        {
            RoomId = roomId;
            MaxPlayers = maxPlayers;
            CurrentPlayers = currentPlayers;
            RoomProperties = roomProperties;
            State = state;
        }

        public RoomInfo()
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
            typeWriter.Write(MaxPlayers);
            typeWriter.Write(CurrentPlayers);
            typeWriter.WriteDictionary(RoomProperties, typeWriter.Write);
            typeWriter.Write((byte)State);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadGuid();
            MaxPlayers = typeReader.ReadInt();
            CurrentPlayers = typeReader.ReadInt();
            RoomProperties = typeReader.ReadDictionary<byte>(typeReader.ReadByte);
            State = (RoomState)typeReader.ReadByte();
        }
    }
}