using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Sockets;

namespace Shaman.Common.Utils.Senders
{
    public interface IPacketQueue : IEnumerable<PacketInfo>
    {
        void Enqueue(byte[] data, int offset, int length, bool isReliable, bool isOrdered);
        void Enqueue(byte[] data, bool isReliable, bool isOrdered);
        int Count { get; }
        bool TryDequeue(out PacketInfo packetInfo);
        void Clear();
    }

    public class PacketQueue : IPacketQueue
    {
        private readonly int _maxPacketSize;
        private readonly IShamanLogger _logger;
        private readonly ConcurrentQueue<PacketInfo> _packetsQueue = new ConcurrentQueue<PacketInfo>();

        public PacketQueue(int maxPacketSize, IShamanLogger logger)
        {
            _maxPacketSize = maxPacketSize;
            _logger = logger;
        }

        public void Enqueue(byte[] data, int offset, int length, bool isReliable, bool isOrdered)
        {
            if (!_packetsQueue.IsEmpty)
            {
                var prevPacket = _packetsQueue.Last();
                if (prevPacket.Length + length <= _maxPacketSize
                    && prevPacket.IsReliable == isReliable
                    && prevPacket.IsOrdered == isOrdered)
                {
                    //add to previous
                    prevPacket.Append(data, offset, length);
                    return;
                }
            }

            //add new packet
            var newPacket = new PacketInfo(data, offset, length, isReliable, isOrdered, _maxPacketSize, _logger);
            _packetsQueue.Enqueue(newPacket);
        }

        public void Enqueue(byte[] data, bool isReliable, bool isOrdered)
        {
            Enqueue(data, 0, data.Length, isReliable, isOrdered);
        }

        public bool TryDequeue(out PacketInfo packetInfo)
        {
            return _packetsQueue.TryDequeue(out packetInfo);
        }

        public int Count => _packetsQueue.Count;

        public void Clear()
        {
            _packetsQueue.Clear();
        }

        public IEnumerator<PacketInfo> GetEnumerator()
        {
            return _packetsQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _packetsQueue.GetEnumerator();
        }
    }
}