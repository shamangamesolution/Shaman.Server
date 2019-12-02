using System.Collections.Generic;

namespace Shaman.MM.Contract
{
    public interface IMatchMakingConfigurator
    {
        void AddMatchMakingGroup(int totalPlayersNeeded, int matchMakingTickMs, bool addBots, bool addOtherPlayers, int timeBeforeBotsAddedMs, int roomClosingInMs,
            Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures);

        void AddRequiredProperty(byte requiredMatchMakingProperty);
    }
}