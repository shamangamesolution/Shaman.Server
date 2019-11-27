using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Game.Bundle
{
    public class MyMessage : MessageBase
    {
        public MyMessage() : base(MessageType.Event, 1)
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