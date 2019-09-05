using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Events
{
    public class ConnectedEvent : EventBase
    {
        
        public ConnectedEvent()
            : base(Messages.CustomOperationCode.Connect)
        {
            
        }

        protected override void SetMessageParameters()
        {
            IsReliable = true;
            IsOrdered = false;
            IsBroadcasted = false;
        }

        protected override void SerializeBody(ISerializer serializer)
        {
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
        }
    }
}