using System;
using System.Collections.Generic;

namespace Shaman.SyncedRepositories
{
    public interface IPlayerRepository
    {
        IEnumerable<int> GetHumanPlayerIndexes();
        bool IsPlayerExist(int playerIndex);
        Guid GetPlayerSessionId(int playerIndex);
    }
}