using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests
{
    public class PingRequest : RequestBase
    {

        public override bool IsReliable => false;
        public override bool IsBroadcasted => false;

        public long SourceTicks { get; set; }

        public PingRequest() : base(CustomOperationCode.PingRequest)
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