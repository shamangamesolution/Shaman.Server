using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Serialization.Extensions;

namespace Shaman.Serialization.Messages.Extensions
{
    public static class CollectionsSerializationExtensions
    {
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

        public static void Write(this ITypeWriter serializer, ConcurrentDictionary<int, ConcurrentDictionary<byte, int>> dict)
        {
            if (dict == null)
                dict = new ConcurrentDictionary<int, ConcurrentDictionary<byte, int>>();
            serializer.Write(dict.Count);

            foreach (var item in dict)
            {
                serializer.Write(item.Key);
                serializer.Write(item.Value.Count);
                foreach (var subItem in item.Value)
                {
                    serializer.Write((byte)subItem.Key);
                    serializer.Write(subItem.Value);
                }
            }
        }
        public static void Write(this ITypeWriter serializer, ConcurrentDictionary<int, ConcurrentDictionary<byte, int?>> dict)
        {
            if (dict == null)
                dict = new ConcurrentDictionary<int, ConcurrentDictionary<byte, int?>>();
            serializer.Write(dict.Count);

            foreach (var item in dict)
            {
                serializer.Write(item.Key);
                serializer.Write(item.Value.Count);
                foreach (var subItem in item.Value)
                {
                    serializer.Write((byte)subItem.Key);
                    serializer.Write(subItem.Value);
                }
            }
        }

        public static void Write(this ITypeWriter serializer, ConcurrentDictionary<int, ConcurrentDictionary<byte, byte>> dict)
        {
            if (dict == null)
                dict = new ConcurrentDictionary<int, ConcurrentDictionary<byte, byte>>();
            serializer.Write(dict.Count);

            foreach (var item in dict)
            {
                serializer.Write(item.Key);
                serializer.Write(item.Value.Count);
                foreach (var subItem in item.Value)
                {
                    serializer.Write((byte)subItem.Key);
                    serializer.Write(subItem.Value);
                }
            }
        }

        public static void Write(this ITypeWriter serializer, ConcurrentDictionary<int, ConcurrentDictionary<byte, byte?>> dict)
        {
            if (dict == null)
                dict = new ConcurrentDictionary<int, ConcurrentDictionary<byte, byte?>>();
            serializer.Write(dict.Count);

            foreach (var item in dict)
            {
                serializer.Write(item.Key);
                serializer.Write(item.Value.Count);
                foreach (var subItem in item.Value)
                {
                    serializer.Write((byte)subItem.Key);
                    serializer.Write(subItem.Value);
                }
            }
        }

        public static void Write(this ITypeWriter serializer, ConcurrentDictionary<int, ConcurrentDictionary<byte, float?>> dict)
        {
            if (dict == null)
                dict = new ConcurrentDictionary<int, ConcurrentDictionary<byte, float?>>();
            serializer.Write(dict.Count);

            foreach (var item in dict)
            {
                serializer.Write(item.Key);
                serializer.Write(item.Value.Count);
                foreach (var subItem in item.Value)
                {
                    serializer.Write((byte)subItem.Key);
                    serializer.Write(subItem.Value);
                }
            }
        }

        public static ConcurrentDictionary<int, ConcurrentDictionary<byte, int>> ReadIntFieldDictionary(this ITypeReader serializer)
        {
            var result = new ConcurrentDictionary<int, ConcurrentDictionary<byte, int>>();
            try
            {
                var cnt = serializer.ReadInt();
                for (int i = 0; i < cnt; i++)
                {
                    var key = serializer.ReadInt();
                    var val = new ConcurrentDictionary<byte, int>();
                    var subCnt = serializer.ReadInt();
                    for (int j = 0; j < subCnt; j++)
                    {
                        var subKey = serializer.ReadByte();
                        var subValue = serializer.ReadInt();
                        val.TryAdd(subKey, subValue);
                    }
                    result.TryAdd(key, val);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing ConcurrentDictionary<int, ConcurrentDictionary<ChangeFieldIndexes, int>>: {e}");
            }

            return result;
        }

        public static ConcurrentDictionary<int, ConcurrentDictionary<byte, int?>> ReadNullableIntFieldDictionary(this ITypeReader serializer)
        {
            var result = new ConcurrentDictionary<int, ConcurrentDictionary<byte, int?>>();
            try
            {
                var cnt = serializer.ReadInt();
                for (int i = 0; i < cnt; i++)
                {
                    var key = serializer.ReadInt();
                    var val = new ConcurrentDictionary<byte, int?>();
                    var subCnt = serializer.ReadInt();
                    for (int j = 0; j < subCnt; j++)
                    {
                        var subKey = serializer.ReadByte();
                        var subValue = serializer.ReadNullableInt();
                        val.TryAdd(subKey, subValue);
                    }
                    result.TryAdd(key, val);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing ConcurrentDictionary<int, ConcurrentDictionary<ChangeFieldIndexes, int?>>: {e}");
            }

            return result;
        }

        public static ConcurrentDictionary<int, ConcurrentDictionary<byte, byte>> ReadByteFieldDictionary(this ITypeReader serializer)
        {
            var result = new ConcurrentDictionary<int, ConcurrentDictionary<byte, byte>>();
            try
            {
                var cnt = serializer.ReadInt();
                for (int i = 0; i < cnt; i++)
                {
                    var key = serializer.ReadInt();
                    var val = new ConcurrentDictionary<byte, byte>();
                    var subCnt = serializer.ReadInt();
                    for (int j = 0; j < subCnt; j++)
                    {
                        var subKey = serializer.ReadByte();
                        var subValue = serializer.ReadByte();
                        val.TryAdd(subKey, subValue);
                    }
                    result.TryAdd(key, val);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing ConcurrentDictionary<int, ConcurrentDictionary<ChangeFieldIndexes, byte>>: {e}");
            }

            return result;
        }

        public static ConcurrentDictionary<int, ConcurrentDictionary<byte, byte?>> ReadNullableByteFieldDictionary(this ITypeReader serializer)
        {
            var result = new ConcurrentDictionary<int, ConcurrentDictionary<byte, byte?>>();
            try
            {
                var cnt = serializer.ReadInt();
                for (int i = 0; i < cnt; i++)
                {
                    var key = serializer.ReadInt();
                    var val = new ConcurrentDictionary<byte, byte?>();
                    var subCnt = serializer.ReadInt();
                    for (int j = 0; j < subCnt; j++)
                    {
                        var subKey = serializer.ReadByte();
                        var subValue = serializer.ReadNullableByte();
                        val.TryAdd(subKey, subValue);
                    }
                    result.TryAdd(key, val);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing ConcurrentDictionary<int, ConcurrentDictionary<ChangeFieldIndexes, byte?>>: {e}");
            }

            return result;
        }

        public static ConcurrentDictionary<int, ConcurrentDictionary<byte, float?>> ReadNullableFloatFieldDictionary(this ITypeReader serializer)
        {
            var result = new ConcurrentDictionary<int, ConcurrentDictionary<byte, float?>>();
            try
            {
                var cnt = serializer.ReadInt();
                for (int i = 0; i < cnt; i++)
                {
                    var key = serializer.ReadInt();
                    var val = new ConcurrentDictionary<byte, float?>();
                    var subCnt = serializer.ReadInt();
                    for (int j = 0; j < subCnt; j++)
                    {
                        var subKey = serializer.ReadByte();
                        var subValue = serializer.ReadNullableFloat();
                        val.TryAdd(subKey, subValue);
                    }
                    result.TryAdd(key, val);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing ConcurrentDictionary<int, ConcurrentDictionary<ChangeFieldIndexes, float?>>: {e}");
            }

            return result;
        }
    }
}