using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;
using Shaman.Messages.RoomFlow;

namespace Shaman.Messages.MM
{
    public class JoinInfoEvent : EventBase
    {
        public JoinInfo JoinInfo { get; set; }
        
        public JoinInfoEvent(JoinInfo joinInfo) : this()//base(CustomOperationCode.JoinInfo)
        {
            JoinInfo = joinInfo;
        }

        public JoinInfoEvent() : base(CustomOperationCode.JoinInfo)
        {
        }

        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.WriteEntity(JoinInfo);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            JoinInfo = serializer.ReadEntity<JoinInfo>();
        }
    }
}