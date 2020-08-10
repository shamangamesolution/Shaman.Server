using System;
using System.Collections.Concurrent;
using Shaman.Common.Utils.Serialization.Pooling;

namespace Shaman.Common.Utils.Senders
{
    public class ShamanStreamPool
    {
        private readonly int _basePacketBufferSize;

        public ShamanStreamPool(int basePacketBufferSize)
        {
            _basePacketBufferSize = basePacketBufferSize;
        }

        public PooledMemoryStream Rent(Type type) 
        {
            return new PooledMemoryStream(GetBufferSize(type));
        }

        public void Return(PooledMemoryStream stream, Type type)
        {
            UpdateBufferSizeStatistics(type, (int) stream.Length);
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

        private int GetBufferSize(Type dtoType)
        {
            if (BufferStatistics.TryGetValue(dtoType, out var size))
                return size;
            return _basePacketBufferSize;
        }

        private static readonly ConcurrentDictionary<Type, int> BufferStatistics = new ConcurrentDictionary<Type, int>();
    }
}