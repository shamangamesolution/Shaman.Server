using System;
using System.Buffers;
using System.Collections.Generic;
using Shaman.Common.Utils;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Udp.Sockets
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
        public DataPacket(byte[] buffer, int offset, int length, DeliveryOptions deliveryOptions)
        {
            Buffer = buffer;
            Offset = offset;
            Length = length;
            DeliveryOptions = deliveryOptions;
        }

        public readonly byte[] Buffer;
        public readonly int Offset;
        public readonly int Length;
        public readonly DeliveryOptions DeliveryOptions;
    }


    /// <summary>
    /// Temporary interface for for clientPeer
    /// </summary>
    public interface IPacketInfo : IDisposable
    {
        bool IsReliable { get; }
        bool IsOrdered { get; }
        byte[] Buffer { get; }
        int Offset { get; }
        int Length { get; }
    }

    public class PacketInfo : OnceDisposable, IPacketInfo
    {
        private readonly IShamanLogger _logger;
        public bool IsReliable { get; }
        public bool IsOrdered { get; }
        public byte[] Buffer { get; }
        public int Offset { get; }
        public int Length { get; private set; }
        private int _disposed = 0;


        public PacketInfo(DeliveryOptions deliveryOptions, int maxPacketSize,
            IShamanLogger logger, Payload payloadPart1, Payload payloadPart2)
            : this(deliveryOptions, maxPacketSize, logger, (int) (payloadPart1.Length + payloadPart2.Length))
        {
            var sumLength = payloadPart1.Length + payloadPart2.Length;
            AppendLength(sumLength);
            AppendPayload(payloadPart1);
            AppendPayload(payloadPart2);
        }

        public PacketInfo(DeliveryOptions deliveryOptions, int maxPacketSize,
            IShamanLogger logger, Payload payload) : this(deliveryOptions, maxPacketSize, logger, (int) payload.Length)
        {
            AppendLength(payload.Length);
            AppendPayload(payload);
        }

        private PacketInfo(DeliveryOptions deliveryOptions, int maxPacketSize,
            IShamanLogger logger, int length)
        {
            _logger = logger;
            Buffer = ArrayPool<byte>.Shared.Rent(Math.Max(maxPacketSize,
                length + 3 /*ushort of single packet length + byte of packets count*/));
            Length = 1 /* packet number byte */;
            Offset = 0;

            Buffer[0] = 1;
            IsReliable = deliveryOptions.IsReliable;
            IsOrdered = deliveryOptions.IsOrdered;
        }

        public void Append(Payload payload)
        {
            IncrementPackets();
            AppendLength(payload.Length);
            AppendPayload(payload);
        }

        public void Append(Payload payloadPart1, Payload payloadPart2)
        {
            IncrementPackets();
            var length = payloadPart1.Length + payloadPart2.Length;
            AppendLength(length);
            AppendPayload(payloadPart1);
            AppendPayload(payloadPart2);
        }

        private void AppendPayload(Payload payload)
        {
            System.Buffer.BlockCopy(payload.Buffer, payload.Offset, Buffer, Length, payload.Length);
            Length += payload.Length;
        }

        private void IncrementPackets()
        {
            if (!AllowAppendPackage())
                throw new Exception("Packages count exceeded");
            Buffer[0]++;
        }

        public bool AllowAppendPackage()
        {
            return Buffer[0] != byte.MaxValue;
        }

        private void AppendLength(int length)
        {
            if (length > ushort.MaxValue)
            {
                Buffer[Length] = 0;
                Buffer[Length + 1] = 0;
                Buffer[Length + 2] = (byte) length;
                Buffer[Length + 3] = (byte) (length >> 8);
                Buffer[Length + 4] = (byte) (length >> 16);
                Buffer[Length + 5] = (byte) (length >> 24);
                Length += 6;
            }
            else
            {
                Buffer[Length] = (byte) length;
                Buffer[Length + 1] = (byte) (length >> 8);
                Length += 2;
            }
        }

        protected override void DisposeImpl()
        {
            ArrayPool<byte>.Shared.Return(Buffer);
        }

        public static IEnumerable<OffsetInfo> GetOffsetInfo(byte[] array, int offset)
        {
            if (array.Length < 1)
                yield break;

            var messageCount = array[offset];
            var totalOffset = offset + 1;

            for (var i = 0; i < messageCount; i++)
            {
                int len = BitConverter.ToUInt16(array, totalOffset);
                totalOffset += 2;
                if (len == 0)
                {
                    len = BitConverter.ToInt32(array, totalOffset);
                    totalOffset += 4;
                }

                yield return new OffsetInfo(totalOffset, len);
                totalOffset += len;
            }
        }
    };
}