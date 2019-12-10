using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Events
{
    public class PingEvent : EventBase
    {
        public override bool IsReliable => false;
        public override bool IsBroadcasted => false;

        public PingEvent()
            : base(Messages.CustomOperationCode.Ping)
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