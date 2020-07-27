using System;

namespace Shaman.Serialization
{
    public interface ITypeReader
    {
        byte ReadByte();
        sbyte ReadSByte();
        ushort ReadUShort();
        bool ReadBool();
        int ReadInt();
        uint ReadUint();
        float ReadFloat();
        short ReadShort();
        long ReadLong();
        ulong ReadULong();
        string ReadString();
        byte[] ReadBytes();
        Guid ReadGuid();
        DateTime ReadDate();
        TimeSpan ReadTimeSpan();
    }
}