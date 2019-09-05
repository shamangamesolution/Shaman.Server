using System.IO;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public abstract class EntityBase : SerializableBase
    {
        //serialization
        protected abstract void SerializeBody(ISerializer serializer);        

        public override byte[] Serialize(ISerializer serializer)
        {
            SerializeBody(serializer);
            var buffer = serializer.GetDataBuffer();
            serializer.GetLogger().Debug($"Serialized entity {this.GetType()}. Current buffer size {serializer.GetCurrentBufferSize()}");
            if (ToFlush)
                serializer.Flush();
            return buffer;
        }
        
        //deserialization
        protected abstract void DeserializeBody(ISerializer serializer);

        private void Deserialize(ISerializer serializer, byte[] param)
        {
            var stream = new MemoryStream(param, 0, param.Length, true);
            serializer.SetStream(stream);
            
            this.DeserializeBody(serializer);
        }
        
        private void Deserialize(ISerializer serializer)
        {
            this.DeserializeBody(serializer);
        }
        
        public static T DeserializeAs<T>(ISerializerFactory serializerFactory, byte[] param)
            where T : EntityBase, new()
        {
            var result = new T();
            var serializer = result.GetSerializer(serializerFactory);
            result.Deserialize(serializer, param);
            return result;
        }   
        
        
        public static T DeserializeAs<T>(ISerializer serializer)
            where T : EntityBase, new()
        {
            var result = new T();
            result.Deserialize(serializer);
            return result;
        } 
    }
}