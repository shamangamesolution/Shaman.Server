using System;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.MM
{
    public class RoomInfoResponse : HttpResponseBase
    {
        public DateTime CreatedDate { get; set; }
        public bool IsOpen { get; set; }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(CreatedDate);
            typeWriter.Write(IsOpen);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            CreatedDate = typeReader.ReadDate();
            IsOpen = typeReader.ReadBool();
        }
    }
}