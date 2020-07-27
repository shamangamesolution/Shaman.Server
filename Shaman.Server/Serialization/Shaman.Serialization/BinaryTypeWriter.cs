using System;
using System.IO;

namespace Shaman.Serialization
{
    public class BinaryTypeWriter : ITypeWriter
    {
        private readonly BinaryWriter _bw;
        
        public BinaryTypeWriter(BinaryWriter writer)
        {
            _bw = writer;
        }

        #region Primitive types

        public void Write(int value)
        {
            if (value <= byte.MaxValue && value >= byte.MinValue)
            {
                _bw.Write((byte)1);
                _bw.Write((byte)value);
            }
            else
            {
                if (value <= short.MaxValue && value >= short.MinValue)
                {
                    _bw.Write((byte)2);
                    _bw.Write((short)value);
                }
                else
                {
                    _bw.Write((byte)3);
                    _bw.Write(value);
                }
            }
        }

        public void Write(byte value)
        {
            _bw.Write(value);
        }

        public void Write(short value)
        {
            _bw.Write(value);
        }

        public void Write(ushort value)
        {
            _bw.Write(value);
        }

        public void Write(uint value)
        {
            _bw.Write(value);
        }

        public void Write(float value)
        {
            _bw.Write(value);
        }

        public void Write(bool value)
        {
            _bw.Write(value);
        }

        public void Write(long value)
        {
            _bw.Write(value);
        }

        public void Write(ulong value)
        {
            _bw.Write(value);
        }

        public void Write(sbyte value)
        {
            _bw.Write(value);
        }

        #endregion
        
        public void Write(byte[] value)
        {
            Write(value.Length);
            _bw.Write(value);
        }
        public void Write(string value)
        {
            if (value == null)
                value = "";
            
            _bw.Write(value);
        }
        
        
        public void Write(Guid value)
        {
            _bw.Write(value.ToByteArray());
        }
        
        public void Write(DateTime dateTime)
        {
            _bw.Write(dateTime.ToBinary());
        }
        public void Write(TimeSpan timeSpan)
        {
            _bw.Write(timeSpan.Ticks);
        }
    }
}