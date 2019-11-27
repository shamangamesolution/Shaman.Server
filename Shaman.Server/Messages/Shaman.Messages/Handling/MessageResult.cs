using Shaman.Common.Utils.Messages;

namespace Shaman.Messages.Handling
{
    public struct MessageResult
    {
        public MessageBase DeserializedMessage;
        public bool Handled;
    }
}