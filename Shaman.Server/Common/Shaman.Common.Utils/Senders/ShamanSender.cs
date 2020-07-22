using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Shaman.Common.Contract;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Pooling;

namespace Shaman.Common.Utils.Senders
{
    public interface IShamanSender
    {
        int Send(ISerializable message, DeliveryOptions deliveryOptions, IPeerSender peer);
        int Send(ISerializable message, DeliveryOptions deliveryOptions, IEnumerable<IPeerSender> peers);
        void CleanupPeerData(IPeerSender peer);
    }

    public class ShamanSender : IShamanSender
    {
        private readonly ISerializer _serializer;
        private readonly IPacketSender _packetSender;
        private readonly IShamanLogger _logger;
        
        // todo realy need base packet size?
        private readonly IPacketSenderConfig _config;
        
        private static readonly ConcurrentDictionary<Type, int> BufferStatistics = new ConcurrentDictionary<Type, int>();

        public ShamanSender(ISerializer serializer, IPacketSender packetSender, IShamanLogger logger,
            IPacketSenderConfig config)
        {
            _serializer = serializer;
            _packetSender = packetSender;
            _logger = logger;
            _config = config;
        }

        public int Send(ISerializable message, DeliveryOptions deliveryOptions, IPeerSender peer)
        {
            using (var memoryStream = new PooledMemoryStream(GetBufferSize(message.GetType()), _logger))
            {
                _serializer.Serialize(message, memoryStream);
                var length = (int) memoryStream.Length;
                _packetSender.AddPacket(peer, deliveryOptions, new Payload(memoryStream.GetBuffer(), 0, length));
                UpdateBufferSizeStatistics(message.GetType(), length);
                return length;
            }
        }

        public int Send(ISerializable message, DeliveryOptions deliveryOptions, IEnumerable<IPeerSender> peers)
        {
            using (var memoryStream = new PooledMemoryStream(_config.GetBasePacketBufferSize(), _logger))
            {
                _serializer.Serialize(message, memoryStream);
                var length = (int) memoryStream.Length;
                var buffer = memoryStream.GetBuffer();
                foreach (var peer in peers)
                {
                    _packetSender.AddPacket(peer, deliveryOptions, new Payload(buffer, 0, length));
                }
                
                UpdateBufferSizeStatistics(message.GetType(), length);
                return length;
            }
        }

        private void UpdateBufferSizeStatistics(Type dtoType, int actualSize)
        {
            var targetValue = (int) (actualSize * 1.5 / 16 + 1) * 16;// pad to 16
            
            if (BufferStatistics.TryGetValue(dtoType, out var statisticsValue))
            {
                if (statisticsValue < targetValue)
                {
                    BufferStatistics.TryUpdate(dtoType, targetValue, statisticsValue);
                }
                
            }
            else
            {
                BufferStatistics.TryAdd(dtoType, targetValue);
            }
        }

        public void CleanupPeerData(IPeerSender peer)
        {
            _packetSender.CleanupPeerData(peer);
        }

        private int GetBufferSize(Type dtoType)
        {
            if (BufferStatistics.TryGetValue(dtoType, out var size))
                return size;
            return _config.GetBasePacketBufferSize();
        }

        
    }
}