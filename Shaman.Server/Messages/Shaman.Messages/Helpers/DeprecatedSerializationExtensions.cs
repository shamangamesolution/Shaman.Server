using System;
using System.Collections.Generic;
using Shaman.Serialization;

namespace Shaman.Messages.Helpers
{
    [Obsolete]
    public static class DeprecatedSerializationExtensions
    {
        /// <summary>
        /// Please, avoid such structures in code
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="dict"></param>
        /// <param name="writeKey"></param>
        /// <typeparam name="TKey"></typeparam>
        public static void WriteDictionary<TKey>(this ITypeWriter writer, Dictionary<TKey, object> dict,
            Action<TKey> writeKey)
        {
            var dictCount = dict.Count;
            writer.Write(dictCount);

            foreach (var entry in dict)
            {
                writeKey(entry.Key);
                WriteObjectValue(writer, entry);
            }
        }

        public static Dictionary<TKey, object> ReadDictionary<TKey>(this ITypeReader reader, Func<TKey> readKey)
        {
            var count = reader.ReadInt();
            var res = new Dictionary<TKey, object>();
            for (int i = 0; i < count; i++)
            {
                var key = readKey();
                var type = SwitchValueToType[reader.ReadByte()];
                var value = SwitchRead[type](reader);
                res.Add(key, value);
            }

            return res;
        }

        private static void WriteObjectValue<TKey>(ITypeWriter writer, KeyValuePair<TKey, object> entry)
        {
            var value = entry.Value;
            if (value == null)
            {
                value = "";
            }

            writer.Write(SwitchTypeToValue[value.GetType()]);


            if (!SwitchWrite.ContainsKey(value.GetType()))
                throw new Exception($"Type {value.GetType()} is not supported by Write method");

            SwitchWrite[value.GetType()](writer, value);
        }


        private static readonly Dictionary<Type, Action<ITypeWriter, object>> SwitchWrite;
        private static readonly Dictionary<Type, byte> SwitchTypeToValue;
        private static readonly Dictionary<Type, Func<ITypeReader, object>> SwitchRead;
        private static readonly Dictionary<byte, Type> SwitchValueToType;

        static DeprecatedSerializationExtensions()
        {
            SwitchWrite = new Dictionary<Type, Action<ITypeWriter, object>>
            {
                {typeof(int), (s, value) => s.Write((int) value)},
                {typeof(byte), (s, value) => s.Write((byte) value)},
                {typeof(short), (s, value) => s.Write((short) value)},
                {typeof(ushort), (s, value) => s.Write((ushort) value)},
                {typeof(uint), (s, value) => s.Write((uint) value)},
                {typeof(float), (s, value) => s.Write((float) value)},
                {typeof(bool), (s, value) => s.Write((bool) value)},
                {typeof(long), (s, value) => s.Write((long) value)},
                {typeof(ulong), (s, value) => s.Write((ulong) value)},
                {typeof(byte[]), (s, value) => s.Write((byte[]) value)},
                {typeof(sbyte), (s, value) => s.Write((sbyte) value)},
                {typeof(string), (s, value) => s.Write((string) value)},
            };

            #region write/read dictionaties helpers

            SwitchRead = new Dictionary<Type, Func<ITypeReader, object>>
            {
                {typeof(int), (s) => s.ReadInt()},
                {typeof(byte), (s) => s.ReadByte()},
                {typeof(short), (s) => s.ReadShort()},
                {typeof(ushort), (s) => s.ReadUShort()},
                {typeof(uint), (s) => s.ReadUint()},
                {typeof(float), (s) => s.ReadFloat()},
                {typeof(bool), (s) => s.ReadBool()},
                {typeof(long), s => s.ReadLong()},
                {typeof(ulong), (s) => s.ReadULong()},
                {typeof(byte[]), (s) => s.ReadBytes()},
                {typeof(sbyte), (s) => s.ReadSByte()},
                {typeof(string), (s) => s.ReadString()},
            };
            SwitchValueToType = new Dictionary<byte, Type>
            {
                {1, typeof(int)},
                {2, typeof(byte)},
                {3, typeof(short)},
                {4, typeof(ushort)},
                {5, typeof(uint)},
                {6, typeof(float)},
                {7, typeof(bool)},
                {8, typeof(long)},
                {9, typeof(ulong)},
                {10, typeof(byte[])},
                {11, typeof(sbyte)},
                {12, typeof(string)},
            };
            SwitchTypeToValue = new Dictionary<Type, byte>
            {
                {typeof(int), 1},
                {typeof(byte), 2},
                {typeof(short), 3},
                {typeof(ushort), 4},
                {typeof(uint), 5},
                {typeof(float), 6},
                {typeof(bool), 7},
                {typeof(long), 8},
                {typeof(ulong), 9},
                {typeof(byte[]), 10},
                {typeof(sbyte), 11},
                {typeof(string), 12},
            };

            #endregion
        }
    }
}