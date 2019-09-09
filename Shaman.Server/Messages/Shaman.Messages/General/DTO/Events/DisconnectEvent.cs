using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Events
{
    public class DisconnectEvent : EventBase
    {
        public DisconnectEvent() 
            : base(Messages.CustomOperationCode.Disconnect)
        {
        }

        protected override void SetMessageParameters()
        {
            IsReliable = true;
            IsOrdered = true;
            IsBroadcasted = true;
        }

        protected override void SerializeBody(ISerializer serializer)
        {
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
        }
    }
}