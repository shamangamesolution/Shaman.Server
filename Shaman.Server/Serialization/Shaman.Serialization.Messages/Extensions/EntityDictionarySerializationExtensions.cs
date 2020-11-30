using System;
using System.Linq;

namespace Shaman.Serialization.Messages.Extensions
{
    public static class EntityDictionarySerializationExtensions
    {
        public static void Write<T>(this ITypeWriter typeWriter, EntityDictionary<T> list)
            where T :EntityBase
        {
            EntityDictionary<T> list1 = list;
            try
            {
                if (list1 == null)
                    list1 = new EntityDictionary<T>();
                
                typeWriter.Write(list1.Count());
                foreach (var item in list1)
                {
                    item.Serialize(typeWriter);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing EntityDictionary<{typeof(T)}>: {e}");
            }
        }

        public static EntityDictionary<T> ReadEntityDictionary<T>(this ITypeReader typeReader)
            where T:EntityBase, new()
        {
            var result = new EntityDictionary<T>();
            
            try
            {
                var length = typeReader.ReadInt();                   
                
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        var item = new T();
                        item.Deserialize(typeReader);
                        result.Add(item);
                        
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error deserializing EntityDictionary<{typeof(T)}>: {e}");
            }

            
            return result;
        }
    }
}