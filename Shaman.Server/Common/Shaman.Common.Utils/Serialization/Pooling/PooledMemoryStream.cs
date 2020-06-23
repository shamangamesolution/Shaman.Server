using System;
using System.Buffers;
using System.IO;
using System.Threading;
using Shaman.Common.Utils.Logging;

namespace Shaman.Common.Utils.Serialization.Pooling
{
    public class PooledMemoryStream : Stream
    {
        private readonly int _baseLength;
        private readonly IShamanLogger _logger;
        private byte[] _buffer;
        private int _writeIndex;

        private int _disposed = 0;
        private bool _wasExtended = false;

        public PooledMemoryStream(int baseLength, IShamanLogger logger)
        {
            _baseLength = baseLength;
            _logger = logger;
            _writeIndex = 0;
            _buffer = ArrayPool<byte>.Shared.Rent(baseLength);
        }

        public override void Flush()
        {
        }

        public override void Close()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
            else
            {
                _logger.Error($"DOUBLE_RENT_RETURN in PooledMemoryStream: wasExtended {_wasExtended}, baseLength {_baseLength}");
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
            _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(_buffer.Length * 2, _buffer.Length + appendingCount));
            Buffer.BlockCopy(oldBuffer, 0, _buffer, 0, _writeIndex);
            ArrayPool<byte>.Shared.Return(oldBuffer);
            _wasExtended = true;
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