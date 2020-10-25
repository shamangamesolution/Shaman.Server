using System;
using System.Collections.Generic;
using Shaman.Messages.Helpers;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.RoomFlow
{
    public class DirectJoinRandomRoomRequest : RequestBase
    {
        public Dictionary<byte, object> RoomProperties { get; set; }
        public Dictionary<byte, object> JoinProperties { get; set; }
        
        public DirectJoinRandomRoomRequest(Dictionary<byte, object> roomProperties, Dictionary<byte, object> joinProperties) 
            : base(Messages.ShamanOperationCode.JoinRandomRoom)
        {
            RoomProperties = roomProperties;
            this.JoinProperties = joinProperties;
        }

        public DirectJoinRandomRoomRequest() 
            : base(Messages.ShamanOperationCode.JoinRandomRoom)
        {
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteDictionary(RoomProperties, typeWriter.Write);
            typeWriter.WriteDictionary(JoinProperties, typeWriter.Write);

        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            RoomProperties = typeReader.ReadDictionary(typeReader.ReadByte);
            JoinProperties = typeReader.ReadDictionary(typeReader.ReadByte);
        }
    }
}