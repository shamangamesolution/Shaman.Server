using System.Collections.Generic;
using Shaman.Contract.MM;

namespace Shaman.Bundling.TestBundle
{
    public class RoomPropertiesProvider : IRoomPropertiesProvider
    {
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 0;
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 0;
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 0;
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return new Dictionary<byte, object>();
        }
    }

    public class TestMmResolver: IMmResolver
    {
        public void Configure(IMatchMakingConfigurator matchMaker)
        {
        }

        public IRoomPropertiesProvider GetRoomPropertiesProvider()
        {
            return new RoomPropertiesProvider();
        }
    }
}