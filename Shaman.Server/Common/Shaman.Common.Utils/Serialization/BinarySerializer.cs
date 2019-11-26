using System.IO;
using System.Text;
using Shaman.Common.Utils.Messages;

namespace Shaman.Common.Utils.Serialization
{
    public class BinarySerializer : ISerializer
    {
        public byte[] Serialize(ISerializable serializable)
        {
            var memoryStream = new MemoryStream();
            using (var bw = new BinaryWriter(memoryStream))
            {
                serializable.Serialize(new BinaryTypeWriter(bw));
            }

            return memoryStream.ToArray();
        }

        public T DeserializeAs<T>(byte[] param)
            where T : ISerializable, new()
        {
            var result = new T();
            using (var reader = new BinaryReader(new MemoryStream(param)))
            {
                var deserializer = new BinaryTypeReader(reader);
                result.Deserialize(deserializer);
            }

            return result;
        }

        public T DeserializeAs<T>(byte[] param, int offset, int length)
            where T : ISerializable, new()
        {
            var result = new T();
            using (var reader = new BinaryReader(new MemoryStream(param, offset, length)))
            {
                var deserializer = new BinaryTypeReader(reader);
                result.Deserialize(deserializer);
            }

            return result;
        }
    }
}