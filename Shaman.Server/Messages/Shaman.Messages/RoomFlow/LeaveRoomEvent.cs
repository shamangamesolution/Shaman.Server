using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public class LeaveRoomEvent : EventBase
    {
        
        public LeaveRoomEvent() 
            : base(Messages.CustomOperationCode.LeaveRoom)
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