using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Messages
{
    public class CustomEvent : EventBase
    {
        public override bool IsReliable => true;
        public byte[] Data { get; set; }

        public CustomEvent(byte[] data) : base(MessageCodes.CustomEvent)
        {
            Data = data;
        }

        public CustomEvent() : base(MessageCodes.CustomEvent)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(Data);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            Data = typeReader.ReadBytes();
        }
    }
}