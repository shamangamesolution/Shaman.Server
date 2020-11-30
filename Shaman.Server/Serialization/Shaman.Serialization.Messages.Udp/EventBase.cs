namespace Shaman.Serialization.Messages.Udp
{
    public abstract class EventBase : MessageBase
    {
        public override bool IsReliable => true;

        protected EventBase(byte operationCode) : base(operationCode)
        {
        }
    }
}