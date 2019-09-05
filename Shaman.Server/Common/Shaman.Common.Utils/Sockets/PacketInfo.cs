using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

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
    
    public class PacketInfo : IDisposable
    {
        public byte OrderId; 
        public IPEndPoint EndPoint;
        public int Offset;
        public int Length;
        public byte[] Buffer;
        public bool ReturnAfterSend;
        public Action RecycleCallback;
        public bool IsReliable;
        public bool IsOrdered;
        public byte PacketsNumber;

        private object _sync = new object();
        private int _maxPacketSize;
        public PacketInfo(int maxPacketSize)
        {
            Length = 1;
            PacketsNumber = 0;
            Buffer = null;
            _maxPacketSize = maxPacketSize;
        }
        
        public void Add(byte[] serializedMessage, bool isReliable, bool isOrdered)
        {
            lock (_sync)
            {
                var serializedLength = (ushort) serializedMessage.Length;
                PacketsNumber++;

                IsReliable = isReliable;
                IsOrdered = isOrdered;

                //return prev
                if (Buffer == null)
                    Buffer = ArrayPool<byte>.Shared.Rent(Math.Max(_maxPacketSize, serializedLength));
                
                Buffer[0] = PacketsNumber;
                Buffer[Length] = (byte) serializedLength;
                Buffer[Length + 1] = (byte) (serializedLength >> 8);
                System.Buffer.BlockCopy(serializedMessage, 0, Buffer, Length + sizeof(ushort), serializedLength);
                Length = Length + sizeof(ushort) + serializedLength;
            }
        }

        public void Dispose()
        {
            if (Buffer != null)
                ArrayPool<byte>.Shared.Return(Buffer);
        }

        public static List<OffsetInfo> GetOffsetInfo(byte[] array, int offset)
        {
            var result = new List<OffsetInfo>();
            if (array.Length < 1)
                return result;

            var messageCount = array[offset];
            var totalOffset = offset + 1;
            
            for (int i = 0; i < messageCount; i++)
            {
                var len = BitConverter.ToUInt16(new byte[2] {array[totalOffset] , array[totalOffset + 1]}, 0);
                result.Add(new OffsetInfo(totalOffset + sizeof(ushort), len));
                totalOffset += sizeof(ushort) + len;
            }
            
            return result;
        }
        
    };
}
