using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public abstract class EntityBase : ISerializable
    {
        public int Id { get; set; }

        public void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write(Id);
            SerializeBody(typeWriter);
        }

        public void Deserialize(ITypeReader typeReader)
        {
            Id = typeReader.ReadInt();
            DeserializeBody(typeReader);
        }
        protected abstract void SerializeBody(ITypeWriter typeWriter);
        protected abstract void DeserializeBody(ITypeReader typeReader);
    }
}