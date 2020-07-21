using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Events
{
    public class DisconnectEvent : EventBase
    {
        public override bool IsReliable => true;

        public DisconnectEvent() 
            : base(Messages.ShamanOperationCode.Disconnect)
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