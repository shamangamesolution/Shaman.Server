using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.Handling
{
    public struct MessageResult
    {
        public MessageBase DeserializedMessage;
        public bool Handled;
    }
}