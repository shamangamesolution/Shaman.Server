using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public abstract class SerializableBase
    {
        protected bool ToFlush = false;

        public byte[] Serialize(ISerializerFactory serializerFactory)
        {
            var serializer = GetSerializer(serializerFactory);
            //set toflush for high level entity
            ToFlush = true;
            return Serialize(serializer);
        }
        
        public abstract byte[] Serialize(ISerializer serializer);
        

        protected virtual ISerializer GetSerializer(ISerializerFactory serializerFactory)
        {
            return serializerFactory.GetBinarySerializer();
        }

    }
}