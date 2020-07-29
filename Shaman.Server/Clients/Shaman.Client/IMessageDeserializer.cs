using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Client
{
    public interface IMessageDeserializer
    {
        MessageBase DeserializeMessage(ushort operationCode, ISerializer serializer, byte[] message);
    }
}