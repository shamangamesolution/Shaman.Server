using System.Collections.Generic;
using Shaman.Contract.MM;

namespace Shaman.MM
{
    class RoomPropertiesProvider : IRoomPropertiesProvider
    {
        internal static IRoomPropertiesProvider RoomPropertiesProviderImplementation;
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return RoomPropertiesProviderImplementation.GetMatchMakingTick(playerMatchMakingProperties);
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return RoomPropertiesProviderImplementation.GetMaximumPlayers(playerMatchMakingProperties);
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return RoomPropertiesProviderImplementation.GetMaximumMatchMakingTime(playerMatchMakingProperties);
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return RoomPropertiesProviderImplementation.GetAdditionalRoomProperties(playerMatchMakingProperties);
        }
    }
}