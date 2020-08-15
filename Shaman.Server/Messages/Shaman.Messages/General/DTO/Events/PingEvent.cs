using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.General.DTO.Events
{
    public class PingEvent : EventBase
    {
        public override bool IsReliable => false;

        public PingEvent()
            : base(Messages.ShamanOperationCode.Ping)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
        }
    }
}