using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
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