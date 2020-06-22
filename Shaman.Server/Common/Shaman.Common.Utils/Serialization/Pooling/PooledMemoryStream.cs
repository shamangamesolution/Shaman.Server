using System;
using System.Buffers;
using System.IO;

namespace Shaman.Common.Utils.Serialization.Pooling
{
    public class PooledMemoryStream : Stream
    {
        private byte[] _buffer;
        private int _writeIndex;

        public PooledMemoryStream(int baseLength)
        {
            _writeIndex = 0;
            _buffer = ArrayPool<byte>.Shared.Rent(baseLength);
        }

        public override void Flush()
        {
        }

        public override void Close()
        {
            ArrayPool<byte>.Shared.Return(_buffer);
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
            _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(_buffer.Length * 2, _buffer.Length + appendingCount));
            Buffer.BlockCopy(oldBuffer, 0, _buffer, 0, _writeIndex);
            ArrayPool<byte>.Shared.Return(oldBuffer);
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