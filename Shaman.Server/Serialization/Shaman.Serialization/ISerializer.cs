using System;
using System.IO;

namespace Shaman.Serialization
{
    public interface ISerializer
    {
        byte[] Serialize(ISerializable serializable);

        T DeserializeAs<T>(byte[] param)
            where T : ISerializable, new();

        T DeserializeAs<T>(byte[] param, int offset, int length)
            where T : ISerializable, new();

        void Serialize(ISerializable serializable, Stream output);

        T DeserializeAs<T>(Stream input)
            where T : ISerializable, new();

        object Deserialize(Stream input, Type type);
        object Deserialize(byte[] param, Type type);
    }
}