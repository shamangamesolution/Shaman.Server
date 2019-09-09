using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.Extensions
{
    public static class SerializerExtensions
    {
        public static void WriteEntityDictionary<T>(this ISerializer serializer, EntityDictionary<T> list)
            where T :EntityBase
        {
            try
            {
                if (list == null)
                    list = new EntityDictionary<T>();
                
                serializer.WriteInt(list.Count());
                foreach (var item in list)
                {
                    item.Serialize(serializer);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing EntityDictionary<{typeof(T)}>: {e}");
            }
        }
        
        public static EntityDictionary<T> ReadEntityDictionary<T>(this ISerializer serializer)
            where T:EntityBase, new()
        {
            var result = new EntityDictionary<T>();
            
            try
            {
                var length = serializer.ReadInt();                   
                
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {

                        var item = EntityBase.DeserializeAs<T>(serializer);
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
        public static void WriteList(this ISerializer serializer, List<int> list)
        {
            if (list == null)
                list = new List<int>();
            serializer.Write(list.Count);
            foreach(var item in list)
                serializer.Write(item);
        }
        
        public static void WriteList(this ISerializer serializer, List<string> list)
        {
            if (list == null)
                list = new List<string>();
            serializer.Write(list.Count);
            foreach(var item in list)
                serializer.Write(item);
        }
        
        public static List<string> ReadStringList(this ISerializer serializer)
        {
            var result = new List<string>();
            
            try
            {
                var length = serializer.ReadInt();                   
                
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        result.Add(serializer.ReadString());
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error deserializing list<string>: {e}");
            }

            
            return result;
        }
        
        public static List<int> ReadIntList(this ISerializer serializer)
        {
            var result = new List<int>();
            
            try
            {
                var length = serializer.ReadInt();                   
                
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        result.Add(serializer.ReadInt());
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error deserializing list<int>: {e}");
            }

            
            return result;
        }
        
        public static void WriteEntity<T>(this ISerializer serializer, T entity)
            where T :EntityBase
        {
            try
            {
                serializer.WriteBool(entity != null);

                if (entity == null)
                    return;
                
                entity.Serialize(serializer);
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing entity <{typeof(T)}>: {e}");
            }
        }
        
        public static T ReadEntity<T>(this ISerializer serializer)
            where T :EntityBase, new()
        {
            try
            {
                if (!serializer.ReadBool())
                    return null;

                return EntityBase.DeserializeAs<T>(serializer);
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing entity <{typeof(T)}>: {e}");
            }
        }
        
        public static void WriteList<T>(this ISerializer serializer, List<T> list)
            where T :EntityBase
        {
            try
            {
                if (list == null)
                    list = new List<T>();
                
                serializer.WriteInt(list.Count);
                foreach (var item in list)
                {
                    item.Serialize(serializer);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing list<{typeof(T)}>: {e}");
            }
        }
        
        public static List<T> ReadList<T>(this ISerializer serializer)
            where T:EntityBase, new()
        {
            var result = new List<T>();
            
            try
            {
                var length = serializer.ReadInt();                   
                
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {

                        var item = EntityBase.DeserializeAs<T>(serializer);
                        result.Add(item);
                        
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error deserializing list<{typeof(T)}>: {e}");
            }

            
            return result;
        }
        public static void WritePlayer(this ISerializer serializer, Player player, SerializationRules serializationRules)
        {
            try
            {
                player.SerializationRules = serializationRules;
                serializer.WriteEntity(player);
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing Player: {e}");
            }
        }
        
        public static Player ReadPlayer(this ISerializer serializer)
        {
            try
            {
                return serializer.ReadEntity<Player>(); //EntityBase.DeserializeAs<Player>(serializer);
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing entity Player: {e}");
            }
        }
    }
}