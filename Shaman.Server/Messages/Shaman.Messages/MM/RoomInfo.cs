using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class RoomInfo : EntityBase
    {
        public Guid RoomId { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public int ClosingInMs { get; set; }
        public Dictionary<byte, object> RoomProperties { get; set; }
        
        public RoomInfo(Guid roomId, int maxPlayers, int currentPlayers, int closingInMs, Dictionary<byte, object> roomProperties)
        {
            RoomId = roomId;
            MaxPlayers = maxPlayers;
            CurrentPlayers = currentPlayers;
            ClosingInMs = closingInMs;
            RoomProperties = roomProperties;
        }

        public RoomInfo()
        {
            
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
            typeWriter.Write(MaxPlayers);
            typeWriter.Write(CurrentPlayers);
            typeWriter.Write(ClosingInMs);
            typeWriter.WriteDictionary(RoomProperties, typeWriter.Write);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadGuid();
            MaxPlayers = typeReader.ReadInt();
            CurrentPlayers = typeReader.ReadInt();
            ClosingInMs = typeReader.ReadInt();
            RoomProperties = typeReader.ReadDictionary<byte>(typeReader.ReadByte);
        }
    }
}