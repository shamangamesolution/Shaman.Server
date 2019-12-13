using System.Collections.Generic;
using Shaman.MM.Providers;

namespace Shaman.MM.Tests.Fakes
{
    public class FakeRoomPropertiesProvider : IRoomPropertiesProvider
    {
        private int _mmTick;
        private int _maxPlayers;
        private int _mmTime;
        
        public FakeRoomPropertiesProvider(int maxPlayers, int mmTime, int mmTick)
        {
            _maxPlayers = maxPlayers;
            _mmTime = mmTime;
            _mmTick = mmTick;
        }
        
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _mmTick;
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _maxPlayers;
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _mmTime;
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties()
        {
            return new Dictionary<byte, object>();
        }
    }
}