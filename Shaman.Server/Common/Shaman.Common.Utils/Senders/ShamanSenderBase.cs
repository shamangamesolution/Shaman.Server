using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization.Pooling;
using Shaman.Serialization;

namespace Shaman.Common.Utils.Senders
{
    // todo need refactoring of sending layer to avoid such artifacts
    // todo consider to move to own bytes pool 
    
    public abstract class ShamanSenderBase<TPeer>
    {
        private readonly ISerializer _serializer;
        private readonly IShamanLogger _logger;
        private readonly int _basePacketBufferSize;

        private static readonly ConcurrentDictionary<Type, int> BufferStatistics = new ConcurrentDictionary<Type, int>();

        public ShamanSenderBase(ISerializer serializer, IShamanLogger logger, int basePacketBufferSize)
        {
            _serializer = serializer;
            _logger = logger;
            _basePacketBufferSize = basePacketBufferSize;
        }

        public int Send(ISerializable message, DeliveryOptions deliveryOptions, TPeer peer)
        {
            using (var memoryStream = new PooledMemoryStream(GetBufferSize(message.GetType()), _logger))
            {
                _serializer.Serialize(message, memoryStream);
                var length = (int) memoryStream.Length;
                Send(deliveryOptions, peer, new Payload(memoryStream.GetBuffer(), 0, length));
                UpdateBufferSizeStatistics(message.GetType(), length);
                return length;
            }
        }

        public int Send(ISerializable message, DeliveryOptions deliveryOptions, IEnumerable<TPeer> peers)
        {
            using (var memoryStream = new PooledMemoryStream(_basePacketBufferSize, _logger))
            {
                _serializer.Serialize(message, memoryStream);
                var length = (int) memoryStream.Length;
                var buffer = memoryStream.GetBuffer();
                foreach (var peer in peers)
                {
                    Send(deliveryOptions, peer, new Payload(buffer, 0, length));
                }
                
                UpdateBufferSizeStatistics(message.GetType(), length);
                return length;
            }
        }

        protected abstract void Send(DeliveryOptions deliveryOptions, TPeer peer, Payload payload);

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

        private int GetBufferSize(Type dtoType)
        {
            if (BufferStatistics.TryGetValue(dtoType, out var size))
                return size;
            return _basePacketBufferSize;
        }
    }
}