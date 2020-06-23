using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Shaman.Common.Utils.Logging;

namespace Shaman.Common.Utils.Sockets
{
    public struct OffsetInfo
    {
        public int Offset;
        public int Length;

        public OffsetInfo(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }
    }

    public struct DataPacket
    {
        public DataPacket(byte[] buffer, int offset, int length, bool isReliable)
        {
            Buffer = buffer;
            Offset = offset;
            Length = length;
            IsReliable = isReliable;
        }

        public readonly byte[] Buffer;
        public readonly int Offset;
        public readonly int Length;
        public readonly bool IsReliable;
    }


    /// <summary>
    /// Temporary interface for for clientPeer
    /// </summary>
    public interface IPacketInfo: IDisposable
    {
        bool IsReliable { get; }
        bool IsOrdered { get; }
        byte[] Buffer { get; }
        int Offset { get; }
        int Length { get; }
    }

    public class PacketInfo : IPacketInfo
    {
        private readonly IShamanLogger _logger;
        public  bool IsReliable{get;}
        public  bool IsOrdered{get;}
        public byte[] Buffer{get;}
        public int Offset{get;}
        public int Length{get; private set; }
        private int _disposed = 0;

        public PacketInfo(byte[] data, bool isReliable, bool isOrdered, int maxPacketSize, IShamanLogger logger) : this(data, 0, data.Length,
            isReliable, isOrdered, maxPacketSize, logger)
        {
        }
        public PacketInfo(byte[] data, int offset, int length, bool isReliable, bool isOrdered, int maxPacketSize,
            IShamanLogger logger)
        {
            _logger = logger;
            Buffer = ArrayPool<byte>.Shared.Rent(Math.Max(maxPacketSize, data.Length + 3));
            Length = 1 /* packet number byte */;
            Offset = 0;
            
            Buffer[0] = 1;
            IsReliable = isReliable;
            IsOrdered = isOrdered;
            AddData(data, offset, length);
        }

        public void Append(byte[] serializedMessage)
        {
            Buffer[0]++;
            AddData(serializedMessage, 0, serializedMessage.Length);
        }
        public void Append(byte[] serializedMessage, int offset, int length)
        {
            Buffer[0]++;
            AddData(serializedMessage, offset, length);
        }

        private void AddData(byte[] serializedMessage, int offset, int length)
        {
            Buffer[Length] = (byte) length;
            Buffer[Length + 1] = (byte) (length >> 8);

            System.Buffer.BlockCopy(serializedMessage, offset, Buffer, Length + sizeof(ushort),
                length);
                
            Length += sizeof(ushort) + length;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
            else
            {
                _logger.Error($"DOUBLE_RENT_RETURN in PacketInfo");
            }
        }

        public static IEnumerable<OffsetInfo> GetOffsetInfo(byte[] array, int offset)
        {
            if (array.Length < 1)
                yield break;

            var messageCount = array[offset];
            var totalOffset = offset + 1;

            for (int i = 0; i < messageCount; i++)
            {
                var len = BitConverter.ToUInt16(new byte[2] {array[totalOffset], array[totalOffset + 1]}, 0);
                yield return new OffsetInfo(totalOffset + sizeof(ushort), len);
                totalOffset += sizeof(ushort) + len;
            }
        }
    };
}