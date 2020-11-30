using System.Collections.Generic;
using Shaman.Contract.MM;

namespace Shaman.MM.Tests.Fakes
{
    public class FakeRoomPropertiesProvider : IRoomPropertiesProvider
    {
        private int _mmTick;
        private readonly int _maximumMmWeight;
        private int _maxPlayers;
        private int _mmTime;
        
        public FakeRoomPropertiesProvider(int maxPlayers, int mmTime, int mmTick, int maximumMmWeight)
        {
            _maxPlayers = maxPlayers;
            _mmTime = mmTime;
            _mmTick = mmTick;
            _maximumMmWeight = maximumMmWeight;
        }
        
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _mmTick;
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _maxPlayers;
        }

        public int GetMaximumMatchMakingWeight(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _maximumMmWeight;
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _mmTime;
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return new Dictionary<byte, object>();
        }
    }
}