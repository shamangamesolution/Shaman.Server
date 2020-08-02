using System.Collections.Generic;

namespace Shaman.Contract.MM
{
    public interface IRoomPropertiesProvider
    {
        int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties);
        int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties);
        int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties);
        Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties);
    }
}