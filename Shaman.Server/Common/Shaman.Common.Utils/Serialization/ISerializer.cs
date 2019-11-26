
using Shaman.Common.Utils.Messages;

namespace Shaman.Common.Utils.Serialization
{
    public interface ISerializer
    {
        byte[] Serialize(ISerializable serializable);

        T DeserializeAs<T>(byte[] param)
            where T : ISerializable, new();

        T DeserializeAs<T>(byte[] param, int offset, int length)
            where T : ISerializable, new();
    }
}