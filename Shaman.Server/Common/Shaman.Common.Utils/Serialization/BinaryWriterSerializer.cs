using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shaman.Common.Utils.Logging;

namespace Shaman.Common.Utils.Serialization
{
    public class BinaryWriterSerializer : ISerializer
    {
        private MemoryStream ms = new MemoryStream();
        private BinaryReader reader;
        private BinaryWriter bw;
        private IShamanLogger _logger;
        
        public BinaryWriterSerializer(IShamanLogger logger)
        {
            _logger = logger;
            switchWrite = new Dictionary<Type, Action<object>>
            {
                {typeof(int), (value) => WriteInt((int) value)},
                {typeof(byte), (value) => WriteByte((byte) value)},
                {typeof(short), (value) => WriteShort((short) value)},
                {typeof(ushort), (value) => WriteUShort((ushort) value)},
                {typeof(uint), (value) => WriteUint((uint) value)},
                {typeof(float), (value) => WriteFloat((float) value)},
                {typeof(bool), (value) => WriteBool((bool) value)},
                {typeof(long), (value) => WriteLong((long) value)},
                {typeof(ulong), (value) => WriteULong((ulong) value)},
                {typeof(byte[]), (value) => WriteBytes((byte[]) value)},
                {typeof(sbyte), (value) => WriteSByte((sbyte) value)},
                {typeof(string), (value) => WriteString((string) value)},
            };
            _logger.Debug($"Serializer {this.GetType()} constructor called");
            bw = new BinaryWriter(ms);
        }

        #region write/read dictionary support
        private Dictionary<Type, Action<object>> switchWrite;
        private Dictionary<Type, byte> switchTypeToValue; 
        private Dictionary<Type, Func<object>> switchRead;
        private Dictionary<byte, Type> switchValueToType;
        #endregion
        
        public void Write(object value)
        {
            if (value == null)
            {
                value = "";
            }

            if (!switchWrite.ContainsKey(value.GetType()))
                throw new Exception($"Type {value.GetType()} is not supported by Write method");
            
            _logger.Debug($"Serializing: writing {value.GetType()}");
            switchWrite[value.GetType()](value);
        }

        public void WriteSByte(sbyte value, byte bitsCount = 8)
        {
            bw.Write(value);
        }

        public void WriteByte(byte value, byte bitsCount = 8)
        {
            bw.Write(value);
        }

        public void Write(byte[] value, int bitsCount)
        {
            bw.Write(value);
        }

        public void Read(byte[] buffer, int bitsCount)
        {
            throw new NotImplementedException();
        }

        public byte ReadByte(int bitsCount = 8)
        {
            return reader.ReadByte();
        }

        public sbyte ReadSByte(int bitsCount = 8)
        {
            return reader.ReadSByte();
        }

        public ushort ReadUShort(int bitsCount = 16)
        {
            return reader.ReadUInt16();
        }

        public bool ReadBool()
        {
            return reader.ReadBoolean();
        }

        public int ReadInt()
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case 1:
                    return (int) reader.ReadByte();
                case 2:
                    return (short) reader.ReadInt16();
                case 3:
                    return  reader.ReadInt32();
            }

            throw new Exception($"Unknown int type while serializing: {type}");
        }

        public uint ReadUint()
        {
            return reader.ReadUInt32();
        }

        public float ReadFloat()
        {
            return reader.ReadSingle();
        }

        public short ReadShort()
        {
            return reader.ReadInt16();
        }

        public long ReadLong()
        {
            return reader.ReadInt64();
        }

        public ulong ReadULong()
        {
            return reader.ReadUInt64();
        }

        public void WriteBool(bool value)
        {
            bw.Write(value);
        }

        public void WriteInt(int value)
        {
            if (value <= byte.MaxValue && value >= byte.MinValue)
            {
                bw.Write((byte)1);
                bw.Write((byte)value);
            }
            else
            {
                if (value <= short.MaxValue && value >= short.MinValue)
                {
                    bw.Write((byte)2);
                    bw.Write((short)value);
                }
                else
                {
                    bw.Write((byte)3);
                    bw.Write(value);
                }
            }
        }

        public void WriteUint(uint value)
        {
            bw.Write(value);
        }

        public void WriteFloat(float value)
        {
            bw.Write(value);
        }

        public void WriteShort(short value)
        {
            bw.Write(value);
        }

        public void WriteUShort(ushort value, int bitsCount = 16)
        {
            bw.Write(value);
        }

        public void WriteLong(long value)
        {
            bw.Write(value);
        }

        public void WriteULong(ulong value)
        {
            bw.Write(value);
        }

        public long GetPos()
        {
            throw new NotImplementedException();
        }

        public int GetBytePos()
        {
            throw new NotImplementedException();
        }

        public void SetStream(Stream stream)
        {
            reader = new BinaryReader(stream);
        }

        public void Flush()
        {
            bw.Flush();
        }

        public void Clear()
        {
            bw.Close();
        }

        public byte[] GetConversionBuffer()
        {
            throw new NotImplementedException();
        }

        public byte[] GetDataBuffer()
        {
            return ms.ToArray();
        }

        public int GetOffset()
        {
            throw new NotImplementedException();
        }

        public int GetMaxSize()
        {
            throw new NotImplementedException();
        }

        public void WriteString(string value)
        {
            if (value == null)
                value = "";
            
            bw.Write(value);
        }

        public string ReadString()
        {
            return reader.ReadString();
        }

        public void WriteBytes(byte[] bytes)
        {
            bw.Write(bytes.Count());
            bw.Write(bytes);
        }

        public byte[] ReadBytes()
        {
            var count = reader.ReadInt32();
            return reader.ReadBytes(count);
        }

        public void WriteDictionary(Dictionary<byte, object> dict)
        {
            switchTypeToValue = new Dictionary<Type, byte>
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
            
            //write dict count
            WriteInt(dict.Count);
            
            foreach (var item in dict)
            {
                //write key
                WriteByte(item.Key);
                //write value type
                WriteByte(switchTypeToValue[item.Value.GetType()]);                
                //write value
                Write(item.Value);
            }
        }

        public Dictionary<byte, object> ReadDictionary()
        {
            #region write/read dictionaties helpers
            switchRead = new Dictionary<Type, Func<object>>
            {
                {typeof(int), () => ReadInt()},
                {typeof(byte), () => ReadByte()},
                {typeof(short), () => ReadShort()},
                {typeof(ushort), () => ReadUShort()},
                {typeof(uint), () => ReadUint()},
                {typeof(float), () => ReadFloat()},
                {typeof(bool), () => ReadBool()},
                {typeof(long), () => ReadLong()},
                {typeof(ulong), () => ReadULong()},
                {typeof(byte[]), () => ReadBytes()},
                {typeof(sbyte), () => ReadSByte()},
                {typeof(string), () => ReadString()},
            };
            switchValueToType = new Dictionary<byte, Type>
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
            #endregion    
            
            var result = new Dictionary<byte, object>();
            
            //read count
            var count = ReadInt();
            for (int i = 0; i++ < count;)
            {
                var key = ReadByte();
                var type = switchValueToType[ReadByte()];
                var value = switchRead[type]();
                result.Add(key, value);
            }

            return result;
        }

        public IShamanLogger GetLogger()
        {
            return _logger;
        }

        public int GetCurrentBufferSize()
        {
            return (int) ms.Length;
        }
    }   
}