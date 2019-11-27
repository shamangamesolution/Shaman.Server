using Shaman.Common.Utils.Messages;

namespace Shaman.Common.Server.Handling
{
    public struct MessageResult
    {
        public MessageBase DeserializedMessage;
        public bool Handled;
    }
}