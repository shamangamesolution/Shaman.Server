namespace Shaman.Common.Utils.Messages
{
    public abstract class EventBase : MessageBase
    {
        public override bool IsReliable => true;
        public override bool IsBroadcasted => true;

        public EventBase(ushort operationCode) : base(operationCode)
        {
        }
    }
}