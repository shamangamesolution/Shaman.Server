using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Client
{
    public interface IMessageDeserializer
    {
        MessageBase DeserializeMessage(ushort operationCode, ISerializer serializer, byte[] message);
    }
}