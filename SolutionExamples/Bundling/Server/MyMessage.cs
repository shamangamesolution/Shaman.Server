using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Server
{
    public class MyMessage : MessageBase
    {
        public MyMessage() : base(20000)
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