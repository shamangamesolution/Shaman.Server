using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public class LeaveRoomEvent : EventBase
    {
        public override bool IsReliable => true;

        public LeaveRoomEvent() 
            : base(Messages.ShamanOperationCode.LeaveRoom)
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