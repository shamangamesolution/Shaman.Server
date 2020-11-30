using System;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.MM
{
    public class RoomInfoResponse : HttpResponseBase
    {
        public DateTime CreatedDate { get; set; }
        public RoomState State { get; set; }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(CreatedDate);
            typeWriter.Write((byte)State);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            CreatedDate = typeReader.ReadDate();
            State = (RoomState) typeReader.ReadByte();
        }
    }
}