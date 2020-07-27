using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.RoomFlow
{
    public class UpdateRoomRequest : HttpRequestBase
    {
        public Guid RoomId { get; set; }
        public Dictionary<Guid, Dictionary<byte, object>> Players { get; private set; }
        
        public UpdateRoomRequest(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players) : base("updateroom")
        {
            RoomId = roomId;
            Players = players;
        }

        public UpdateRoomRequest() : base("updateroom")
        {
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
            typeWriter.Write(Players.Count);
            foreach (var property in Players)
            {
                typeWriter.Write(property.Key);
                typeWriter.WriteDictionary(property.Value, typeWriter.Write);
            }
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadGuid();
            Players = new Dictionary<Guid, Dictionary<byte, object>>();
            var count = typeReader.ReadInt();
            for (var i = 0; i < count; i++)
            {
                var key = typeReader.ReadGuid();
                var val = typeReader.ReadDictionary(typeReader.ReadByte);
                Players.Add(key, val);
            }
        }
    }
}