using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Events
{
    public class PingEvent : EventBase
    {
        
        public PingEvent()
            : base(Messages.CustomOperationCode.Ping)
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