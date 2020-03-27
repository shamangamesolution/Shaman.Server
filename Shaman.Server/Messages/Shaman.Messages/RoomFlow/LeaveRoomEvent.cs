using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public class LeaveRoomEvent : EventBase
    {
        public override bool IsReliable => true;
        public override bool IsBroadcasted => true;

        public LeaveRoomEvent() 
            : base(Messages.CustomOperationCode.LeaveRoom)
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