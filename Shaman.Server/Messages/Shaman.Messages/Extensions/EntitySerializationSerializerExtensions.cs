using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.Extensions
{
    public static class EntitySerializationSerializerExtensions
    {
        public static void WriteEntityDictionary<T>(this ITypeWriter typeWriter, EntityDictionary<T> list)
            where T :EntityBase
        {
            try
            {
                if (list == null)
                    list = new EntityDictionary<T>();
                
                typeWriter.Write(list.Count());
                foreach (var item in list)
                {
                    item.Serialize(typeWriter);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing EntityDictionary<{typeof(T)}>: {e}");
            }
        }
        public static void Write<T>(this ITypeWriter typeWriter, EntityDictionary<T> list)
            where T :EntityBase
        {
            WriteEntityDictionary<T>(typeWriter, list);
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