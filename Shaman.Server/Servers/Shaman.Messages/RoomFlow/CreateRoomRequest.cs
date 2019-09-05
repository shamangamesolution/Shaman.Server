using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public class CreateRoomRequest : RequestBase
    {
        public Dictionary<byte, object> Properties { get; private set; }
        public Dictionary<Guid, Dictionary<byte, object>> Players { get; private set; }
        
        public CreateRoomRequest(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players) : base(Messages.CustomOperationCode.CreateRoom, "createroom")
        {
            Players = players;
            Properties = properties;
        }

        public CreateRoomRequest() : base(Messages.CustomOperationCode.CreateRoom)
        {
            Properties = new Dictionary<byte, object>();
            Players = new Dictionary<Guid, Dictionary<byte, object>>();
        }
        
        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.WriteDictionary(Properties);
            serializer.WriteInt(Players.Count);
            foreach (var property in Players)
            {
                serializer.WriteBytes(property.Key.ToByteArray());
                serializer.WriteDictionary(property.Value);
            }
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            Properties = serializer.ReadDictionary();
            Players = new Dictionary<Guid, Dictionary<byte, object>>();
            var count = serializer.ReadInt();
            for (var i = 0; i < count; i++)
            {
                var key = new Guid(serializer.ReadBytes());
                var val = serializer.ReadDictionary();
                Players.Add(key, val);
            }
        }
    }
}