using System;

namespace Shaman.Serialization.Messages.Extensions
{
    public static class EntitySerializationExtensions
    {
        public static void WriteEntity<T>(this ITypeWriter typeWriter, T entity)
            where T :EntityBase
        {
            try
            {
                typeWriter.Write(entity != null);

                if (entity == null)
                    return;
                
                entity.Serialize(typeWriter);
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing entity <{typeof(T)}>: {e}");
            }
        }
        
        public static T ReadEntity<T>(this ITypeReader typeReader)
            where T :EntityBase, new()
        {
            try
            {
                if (!typeReader.ReadBool())
                    return null;

                var item = new T();
                item.Deserialize(typeReader);
                return item;
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing entity <{typeof(T)}>: {e}");
            }
        }
    }
}