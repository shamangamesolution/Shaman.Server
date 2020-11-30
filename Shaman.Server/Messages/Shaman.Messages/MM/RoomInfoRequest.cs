using System;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.MM
{
    public class RoomInfoRequest : HttpRequestBase
    {
        public Guid RoomId { get; set; }
        
        public RoomInfoRequest() : base("getroominfo")
        {
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadGuid();
        }
    }
}