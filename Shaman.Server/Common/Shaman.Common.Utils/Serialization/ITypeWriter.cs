using System;

namespace Shaman.Common.Utils.Serialization
{
    public interface ITypeWriter
    {
        void Write(int value);
        void Write(byte value);
        void Write(short value);
        void Write(ushort value);
        void Write(uint value);
        void Write(float value);
        void Write(bool value);
        void Write(long value);
        void Write(ulong value);
        void Write(byte[] value);
        void Write(sbyte value);
        void Write(string value);
        void Write(Guid value);
        void Write(DateTime dateTime);
        void Write(TimeSpan timeSpan);
    }
}