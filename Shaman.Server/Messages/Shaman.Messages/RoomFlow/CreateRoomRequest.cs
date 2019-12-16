using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public class CreateRoomRequest : HttpRequestBase
    {
        public Dictionary<byte, object> Properties { get; private set; }
        public Dictionary<Guid, Dictionary<byte, object>> Players { get; private set; }
        
        public CreateRoomRequest(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players) : base("createroom")
        {
            Players = players;
            Properties = properties;
        }

        public CreateRoomRequest() : base("createroom")
        {
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteDictionary(Properties, typeWriter.Write);
            typeWriter.Write(Players.Count);
            foreach (var property in Players)
            {
                typeWriter.Write(property.Key);
                typeWriter.WriteDictionary(property.Value, typeWriter.Write);
            }
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            Properties = typeReader.ReadDictionary(typeReader.ReadByte);
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