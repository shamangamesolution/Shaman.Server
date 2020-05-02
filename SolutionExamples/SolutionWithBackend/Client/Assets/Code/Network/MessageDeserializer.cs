using Sample.Shared.Data;
using Shaman.Client;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Code.Network
{
    public class MessageDeserializer : IMessageDeserializer
    {
        public MessageBase DeserializeMessage(ushort operationCode, ISerializer serializer, byte[] message)
        {
            return MessageFactory.DeserializeMessage(operationCode, serializer, message);
        }
    }
}
