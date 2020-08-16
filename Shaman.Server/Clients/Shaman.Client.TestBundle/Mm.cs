using System.Collections.Generic;
using Shaman.Contract.MM;

namespace Shaman.Client.TestBundle
{
    public class Mm:IMmResolver
    {
        public void Configure(IMatchMakingConfigurator configurator)
        {
        }

        public IRoomPropertiesProvider GetRoomPropertiesProvider()
        {
            return new RoomPropsProvider();
        }
    }

    public class RoomPropsProvider:IRoomPropertiesProvider
    {
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 1000;
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 1;
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 1000;
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return new Dictionary<byte, object>();
        }
    }
}