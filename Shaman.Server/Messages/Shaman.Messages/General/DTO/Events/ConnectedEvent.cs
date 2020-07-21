using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Events
{
    public class ConnectedEvent : EventBase
    {
        public override bool IsReliable => true;
        
        public ConnectedEvent()
            : base(Messages.ShamanOperationCode.Connect)
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