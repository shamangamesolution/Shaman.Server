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
        public void Serialize(ISerializable serializable, Stream output)
        {
            var bw = new BinaryWriter(output);
            serializable.Serialize(new BinaryTypeWriter(bw));
            bw.Flush();
        }

        public T DeserializeAs<T>(Stream input)
            where T : ISerializable, new()
        {
            var result = new T();
            using (var reader = new BinaryReader(input))
            {
                var deserializer = new BinaryTypeReader(reader);
                result.Deserialize(deserializer);
            }

            return result;
        }
        public T DeserializeAs<T>(byte[] param)
            where T : ISerializable, new()
        {
            return DeserializeAs<T>(new MemoryStream(param));
        }

        public T DeserializeAs<T>(byte[] param, int offset, int length)
            where T : ISerializable, new()
        {
            return DeserializeAs<T>(new MemoryStream(param, offset, length));
        }
    }
}