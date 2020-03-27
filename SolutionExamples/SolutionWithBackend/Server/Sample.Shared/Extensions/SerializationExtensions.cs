using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;

namespace Sample.Shared.Extensions
{
    public static class SerializationExtensions
    {
        public static void Write(this ITypeWriter typeWriter, uint? value)
        {
            if (value == null)
                typeWriter.Write(false);
            else
            {
                typeWriter.Write(true);
                typeWriter.Write(value.Value);
            }
        }
        
        public static uint? ReadNullableUint(this ITypeReader typeReader)
        {
            var notIsNull = typeReader.ReadBool();
            if (notIsNull)
                return typeReader.ReadUint();

            return null;
        }
        public static void Write(this ITypeWriter typeWriter, ushort? value)
        {
            if (value == null)
                typeWriter.Write(false);
            else
            {
                typeWriter.Write(true);
                typeWriter.Write(value.Value);
            }
        }
        
        public static ushort? ReadNullableUshort(this ITypeReader typeReader)
        {
            var notIsNull = typeReader.ReadBool();
            if (notIsNull)
                return typeReader.ReadUShort();

            return null;
        }
        
        public static void Write(this ITypeWriter typeWriter, int? value)
        {
            if (value == null)
                typeWriter.Write(false);
            else
            {
                typeWriter.Write(true);
                typeWriter.Write(value.Value);
            }
        }
        
        public static int? ReadNullableInt(this ITypeReader typeReader)
        {
            var notIsNull = typeReader.ReadBool();
            if (notIsNull)
                return typeReader.ReadInt();

            return null;
        }
        
        public static void Write(this ITypeWriter typeWriter, float? value)
        {
            if (value == null)
                typeWriter.Write(false);
            else
            {
                typeWriter.Write(true);
                typeWriter.Write(value.Value);
            }
        }
        
        public static float? ReadNullableFloat(this ITypeReader typeReader)
        {
            var notIsNull = typeReader.ReadBool();
            if (notIsNull)
                return typeReader.ReadFloat();

            return null;
        }
        
        public static void Write(this ITypeWriter typeWriter, HashSet<byte> hashSet)
        {
            try
            {
                if (hashSet == null)
                    hashSet = new HashSet<byte>();
                
                typeWriter.Write(hashSet.Count());
                foreach (var item in hashSet)
                {
                    typeWriter.Write(item);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing HashSet<byte>: {e}");
            }
        }
        
        public static void Write(this ITypeWriter typeWriter, Dictionary<int, byte> dict)
        {
            try
            {
                if (dict == null)
                    dict = new Dictionary<int, byte>();
                
                typeWriter.Write(dict.Count());
                foreach (var item in dict)
                {
                    typeWriter.Write(item.Key);
                    typeWriter.Write(item.Value);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing Dictionary<int, byte>: {e}");
            }
        }
        
        public static void Write(this ITypeWriter typeWriter, HashSet<int> hashSet)
        {
            try
            {
                if (hashSet == null)
                    hashSet = new HashSet<int>();
                
                typeWriter.Write(hashSet.Count());
                foreach (var item in hashSet)
                {
                    typeWriter.Write(item);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing HashSet<int>: {e}");
            }
        }
        
        public static Dictionary<int, byte> ReadIntByteDictionary(this ITypeReader typeReader)
        {
            var result = new Dictionary<int, byte>();
            
            try
            {
                var length = typeReader.ReadInt();                   
                
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        var key = typeReader.ReadInt();
                        var value = typeReader.ReadByte();
                        result.Add(key, value);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error deserializing HashSet<byte>: {e}");
            }

            return result;
        }
        
        public static HashSet<byte> ReadByteHashSet(this ITypeReader typeReader)
        {
            var result = new HashSet<byte>();
            
            try
            {
                var length = typeReader.ReadInt();                   
                
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        result.Add(typeReader.ReadByte());
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error deserializing HashSet<byte>: {e}");
            }

            return result;
        }
        
        public static HashSet<int> ReadIntHashSet(this ITypeReader typeReader)
        {
            var result = new HashSet<int>();
            
            try
            {
                var length = typeReader.ReadInt();                   
                
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        result.Add(typeReader.ReadInt());
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error deserializing HashSet<int>: {e}");
            }

            return result;
        }
        
        #region lists
        public static void WriteList(this ITypeWriter serializer, List<int> list)
        {
            if (list == null)
                list = new List<int>();
            serializer.Write(list.Count);
            foreach(var item in list)
                serializer.Write(item);
        }
        public static void WriteList(this ITypeWriter serializer, List<ushort> list)
        {
            if (list == null)
                list = new List<ushort>();
            serializer.Write(list.Count);
            foreach(var item in list)
                serializer.Write(item);
        }
        public static void WriteList(this ITypeWriter serializer, List<string> list)
        {
            if (list == null)
                list = new List<string>();
            serializer.Write(list.Count);
            foreach(var item in list)
                serializer.Write(item);
        }
        public static List<string> ReadStringList(this ITypeReader serializer)
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
        public static List<int> ReadIntList(this ITypeReader serializer)
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
        public static List<ushort> ReadUshortList(this ITypeReader serializer)
        {
            var result = new List<ushort>();
            
            try
            {
                var length = serializer.ReadInt();                   
                
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        result.Add(serializer.ReadUShort());
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error deserializing list<ushort>: {e}");
            }

            
            return result;
        }
        #endregion
       
        #region dictionaries
        public static Dictionary<byte, TValue> ReadByteDictionary<TValue>(this ITypeReader serializer)
            where TValue :EntityBase, new()
        {
            var result = new Dictionary<byte, TValue>();

            try
            {
                
                var cnt = serializer.ReadInt();
                for (int i = 0; i < cnt; i++)
                {
                    var key = serializer.ReadByte();
                    var value = serializer.ReadEntity<TValue>();
                    result.Add(key, value);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing Dictionary<byte, {typeof(TValue)}>: {e}");
            }

            return result;
        }
        
        public static Dictionary<ushort, TValue> ReadUShortDictionary<TValue>(this ITypeReader serializer)
            where TValue :EntityBase, new()
        {
            var result = new Dictionary<ushort, TValue>();

            try
            {
                var cnt = serializer.ReadInt();
                for (int i = 0; i < cnt; i++)
                {
                    var key = serializer.ReadUShort();
                    var value = serializer.ReadEntity<TValue>();
                    result.Add(key, value);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing Dictionary<ushort, {typeof(TValue)}>: {e}");
            }

            return result;
        }
        #endregion
    }
}