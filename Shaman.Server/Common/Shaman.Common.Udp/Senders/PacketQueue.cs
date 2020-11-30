using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Udp.Senders
{
    /// <summary>
    /// Not thread safe
    /// </summary>
    public interface IPacketQueue : IEnumerable<PacketInfo>
    {
        int Count { get; }
        bool TryDequeue(out PacketInfo packetInfo);
        void Clear();
        void Enqueue(DeliveryOptions deliveryOptions, Payload payload);
        void Enqueue(DeliveryOptions deliveryOptions, Payload payload1, Payload payload2);
    }

    public class PacketQueue : IPacketQueue
    {
        private readonly int _maxPacketSize;
        private readonly IShamanLogger _logger;
        private readonly Queue<PacketInfo> _packetsQueue = new Queue<PacketInfo>();

        public PacketQueue(int maxPacketSize, IShamanLogger logger)
        {
            _maxPacketSize = maxPacketSize;
            _logger = logger;
        }

        public void Enqueue(DeliveryOptions deliveryOptions, Payload payload)
        {
            var length = payload.Length;
            if (TryGetMatchedPacket(deliveryOptions, length, out var packetInfo))
            {
                packetInfo.Append(payload);
            }
            else
            {
                //add new packet
                var newPacket = new PacketInfo(deliveryOptions, _maxPacketSize, _logger, payload);
                _packetsQueue.Enqueue(newPacket);
            }
        }
        public void Enqueue(DeliveryOptions deliveryOptions, Payload payload1, Payload payload2)
        {
            var length = payload1.Length + payload2.Length;
            if (TryGetMatchedPacket(deliveryOptions, length, out var packetInfo))
            {
                packetInfo.Append(payload1, payload2);
            }
            else
            {
                //add new packet
                var newPacket = new PacketInfo(deliveryOptions, _maxPacketSize, _logger, payload1, payload2);
                _packetsQueue.Enqueue(newPacket);
            }
        }

        private bool TryGetMatchedPacket(DeliveryOptions deliveryOptions,  int length, out PacketInfo packetInfo)
        {
            if (_packetsQueue.Count > 0)
            {
                var prevPacket = _packetsQueue.Last();
                if (prevPacket.Length + length <= _maxPacketSize
                    && prevPacket.IsReliable == deliveryOptions.IsReliable
                    && prevPacket.IsOrdered == deliveryOptions.IsOrdered)
                {
                    packetInfo = prevPacket;
                    return true;
                }
            }

            packetInfo = null;
            return false;
        }

        public bool TryDequeue(out PacketInfo packetInfo)
        {
            if (_packetsQueue.Count > 0)
            {
                packetInfo = _packetsQueue.Dequeue();
                return true;
            }

            packetInfo = null;
            return false;
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