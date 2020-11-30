using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Events
{
    public class ConnectedEvent : EventBase
    {
        public override bool IsReliable => true;
        public override bool IsBroadcasted => false;

        
        public ConnectedEvent()
            : base(Messages.CustomOperationCode.Connect)
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