using System.Collections.Generic;
using Shaman.Messages;
using Shaman.MM.Contract;

namespace Sample.MM
{
    public class RoomPropertiesProvider : IRoomPropertiesProvider
    {
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 250;
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 4;
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 2000;
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties)
        {
            var result = new Dictionary<byte, object>();
            if (playerMatchMakingProperties.ContainsKey(PropertyCode.PlayerProperties.GameMode))
                result.Add(PropertyCode.RoomProperties.GameMode, playerMatchMakingProperties[PropertyCode.PlayerProperties.GameMode]);
            return result;
        }
    }
    
    public class MyMmResolver : IMmResolver
    {
        public void Configure(IMatchMakingConfigurator matchMaker)
        {
            matchMaker.AddRequiredProperty(PropertyCode.PlayerProperties.GameMode);
        }

        public IRoomPropertiesProvider GetRoomPropertiesProvider()
        {
            return new RoomPropertiesProvider();
        }
    }
}