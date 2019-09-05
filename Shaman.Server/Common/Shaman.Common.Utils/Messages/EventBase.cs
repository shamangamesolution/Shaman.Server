namespace Shaman.Common.Utils.Messages
{
    public abstract class EventBase : MessageBase
    {

        protected override void SetMessageParameters()
        {
            IsReliable = true;
            IsOrdered = true;
            IsBroadcasted = true;
        }
        
        public EventBase(ushort operationCode) : base(MessageType.Event, operationCode)
        {
        }
    }
}