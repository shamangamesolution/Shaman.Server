using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

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