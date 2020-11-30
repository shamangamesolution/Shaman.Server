using System;
using System.Buffers;
using System.IO;
using System.Threading;

namespace Shaman.Serialization.Utils.Pooling
{

    public interface IArrayPool
    {
        byte[] Rent(int length);
        void Return(byte[] array);
    }

    public class ArrayPool : IArrayPool
    {
        public byte[] Rent(int length)
        {
            return ArrayPool<byte>.Shared.Rent(length);   
        }
        public void Return(byte[] array)
        {
            ArrayPool<byte>.Shared.Return(array);
        }
        
    }

    public class PooledMemoryStream : Stream
    {
        private readonly IArrayPool _arrayPool;
        private byte[] _buffer;
        private int _writeIndex;

        private int _disposed = 0;

        public PooledMemoryStream(IArrayPool arrayPool, int baseLength)
        {
            _arrayPool = arrayPool;
            _writeIndex = 0;
            _buffer = arrayPool.Rent(baseLength);
        }

        public override void Flush()
        {
        }

        public override void Close()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                _arrayPool.Return(_buffer);
            }
            base.Close();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_buffer.Length - _writeIndex < count)
                ExpandBuffer(count);
            Buffer.BlockCopy(buffer, offset, _buffer, _writeIndex, count);
            _writeIndex += count;
        }

        private void ExpandBuffer(int appendingCount)
        {
            var oldBuffer = _buffer;
            _buffer = _arrayPool.Rent(Math.Max(_buffer.Length * 2, _buffer.Length + appendingCount));
            Buffer.BlockCopy(oldBuffer, 0, _buffer, 0, _writeIndex);
            _arrayPool.Return(oldBuffer);
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _writeIndex;

        public byte[] GetBuffer()
        {
            return _buffer;
        }

        public override long Position
        {
            get => _writeIndex;
            set => _writeIndex = (int) value;
        }
    }
}