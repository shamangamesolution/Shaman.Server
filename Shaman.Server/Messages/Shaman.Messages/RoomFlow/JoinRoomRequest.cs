using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public class JoinRoomRequest : RequestBase
    {
        public Guid RoomId { get; set; }
        public Dictionary<byte, object> Properties { get; set; }
        
        public JoinRoomRequest(Guid roomId, Dictionary<byte, object> properties) 
            : base(Messages.CustomOperationCode.JoinRoom)
        {
            this.RoomId = roomId;
            this.Properties = properties;
        }

        public JoinRoomRequest() 
            : base(Messages.CustomOperationCode.JoinRoom)
        {
        }

        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.WriteBytes(RoomId.ToByteArray());
            serializer.WriteDictionary(Properties);

        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            RoomId = new Guid(serializer.ReadBytes());
            Properties = serializer.ReadDictionary();
        }
    }
}