using System;
using System.IO;

namespace Shaman.Common.Utils.Serialization
{
    public class BinaryTypeReader : ITypeReader
    {
        private readonly BinaryReader _reader;

        public BinaryTypeReader(BinaryReader reader)
        {
            _reader = reader;
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return _reader.ReadSByte();
        }

        public ushort ReadUShort()
        {
            return _reader.ReadUInt16();
        }

        public bool ReadBool()
        {
            return _reader.ReadBoolean();
        }

        public int ReadInt()
        {
            var type = _reader.ReadByte();
            switch (type)
            {
                case 1:
                    return _reader.ReadByte();
                case 2:
                    return _reader.ReadInt16();
                case 3:
                    return _reader.ReadInt32();
            }

            throw new Exception($"Unknown int type while serializing: {type}");
        }

        public uint ReadUint()
        {
            return _reader.ReadUInt32();
        }

        public float ReadFloat()
        {
            return _reader.ReadSingle();
        }

        public short ReadShort()
        {
            return _reader.ReadInt16();
        }

        public long ReadLong()
        {
            return _reader.ReadInt64();
        }

        public ulong ReadULong()
        {
            return _reader.ReadUInt64();
        }

        public string ReadString()
        {
            return _reader.ReadString();
        }

        public byte[] ReadBytes()
        {
            var count = ReadInt();
            return _reader.ReadBytes(count);
        }

        public Guid ReadGuid()
        {
            return new Guid(_reader.ReadBytes(16));
        }

        public DateTime ReadDate()
        {
            return DateTime.FromBinary(_reader.ReadInt64());
        }

        public TimeSpan ReadTimeSpan()
        {
            return TimeSpan.FromTicks(_reader.ReadInt64());
        }
    }
}