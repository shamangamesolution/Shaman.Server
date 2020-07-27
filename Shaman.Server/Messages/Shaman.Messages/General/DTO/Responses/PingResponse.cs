using System;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.General.DTO.Responses
{
    public class PingResponse : ResponseBase
    {
        public override bool IsReliable => false;

        public long SourceTicks { get; set; }

        public PingResponse() : base(ShamanOperationCode.PingResponse)
        {
        }

        public TimeSpan GetElapsedForNow()
        {
            return TimeSpan.FromTicks(DateTime.UtcNow.Ticks - SourceTicks);
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(SourceTicks);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            SourceTicks = typeReader.ReadLong();
        }
    }
}