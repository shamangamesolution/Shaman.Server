using System.Collections.Generic;
using System.IO;
using Shaman.Common.Utils.Logging;

namespace Shaman.Common.Utils.Serialization
{
    public interface ISerializer
    {
        
        void Write(object value);
        void WriteSByte(sbyte value, byte bitsCount = 8);

        void WriteByte(byte value, byte bitsCount = 8);
        void Write(byte[] value, int bitsCount);
        void Read(byte[] buffer, int bitsCount);
        byte ReadByte(int bitsCount = 8);
        sbyte ReadSByte(int bitsCount = 8);

        ushort ReadUShort(int bitsCount = 16);
        bool ReadBool();
        int ReadInt();
        uint ReadUint();
        float ReadFloat();
        short ReadShort();
        long ReadLong();
        ulong ReadULong();
        void WriteBool(bool value);
        void WriteInt(int value);
        void WriteUint(uint value);
        void WriteFloat(float value);
        void WriteShort(short value);
        void WriteUShort(ushort value, int bitsCount = 16);
        void WriteLong(long value);
        void WriteULong(ulong value);
        long GetPos();
        int GetBytePos();
        void SetStream(Stream stream);
        void Flush();
        void Clear();
        byte[] GetConversionBuffer();
        byte[] GetDataBuffer();
        int GetOffset();
        int GetMaxSize();
        void WriteString(string value);
        string ReadString();
        void WriteBytes(byte[] bytes);
        byte[] ReadBytes();
        void WriteDictionary(Dictionary<byte, object> dict);
        Dictionary<byte, object> ReadDictionary();
        IShamanLogger GetLogger();
        int GetCurrentBufferSize();
    }
}