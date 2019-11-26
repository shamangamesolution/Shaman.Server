using System;
using System.Buffers;
using System.Collections.Generic;

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
        public byte[] Buffer;
        public int Offset;
        public int Length;
    }


    /// <summary>
    /// Temporary interface for for clientPeer
    /// </summary>
    public interface IPacketInfo
    {
        bool IsReliable { get; }
        bool IsOrdered { get; }
        byte[] Buffer { get; }
        int Offset { get; }
        int Length { get; }
        void Dispose();
    }

    public class PacketInfo : IPacketInfo
    {
        public  bool IsReliable{get;}
        public  bool IsOrdered{get;}
        public byte[] Buffer{get;}
        public int Offset{get;}
        public int Length{get; private set; }

        public PacketInfo(byte[] data, bool isReliable, bool isOrdered, int maxPacketSize)
        {
            Buffer = ArrayPool<byte>.Shared.Rent(Math.Max(maxPacketSize, data.Length + 3));
            Length = 1 /* packet number byte */;
            Offset = 0;
            
            Buffer[0] = 1;
            IsReliable = isReliable;
            IsOrdered = isOrdered;
            AddData(data);
        }

        public void Append(byte[] serializedMessage)
        {
            Buffer[0]++;
            AddData(serializedMessage);
        }

        private void AddData(byte[] serializedMessage)
        {
            var serializedLength = (ushort) serializedMessage.Length;

            Buffer[Length] = (byte) serializedLength;
            Buffer[Length + 1] = (byte) (serializedLength >> 8);

            System.Buffer.BlockCopy(serializedMessage, 0, Buffer, Length + sizeof(ushort),
                serializedLength);
                
            Length += sizeof(ushort) + serializedLength;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(Buffer);
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