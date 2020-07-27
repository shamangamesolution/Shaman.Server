using System;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.General.DTO.Requests
{
    public class PingRequest : RequestBase
    {

        public override bool IsReliable => false;

        public long SourceTicks { get; set; }

        public PingRequest() : base(ShamanOperationCode.PingRequest)
        {
            SourceTicks = DateTime.UtcNow.Ticks;
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(SourceTicks);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            SourceTicks = typeReader.ReadLong();
        }
    }
}