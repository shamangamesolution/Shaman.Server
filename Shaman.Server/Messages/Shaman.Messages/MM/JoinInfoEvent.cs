using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Messages.Extensions;
using Shaman.Messages.RoomFlow;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class JoinInfoEvent : EventBase
    {
        public JoinInfo JoinInfo { get; set; }
        
        public JoinInfoEvent(JoinInfo joinInfo) : this()//base(CustomOperationCode.JoinInfo)
        {
            JoinInfo = joinInfo;
        }

        public JoinInfoEvent() : base(ShamanOperationCode.JoinInfo)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteEntity(JoinInfo);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            JoinInfo = typeReader.ReadEntity<JoinInfo>();
        }
    }
}