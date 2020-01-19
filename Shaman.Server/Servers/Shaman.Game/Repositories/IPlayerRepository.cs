using System;
using System.Collections.Generic;

namespace Shaman.Game.Repositories
{
    public interface IPlayerRepository
    {
        IEnumerable<int> GetHumanPlayerIndexes();
        bool IsPlayerExist(int playerIndex);
        Guid GetPlayerSessionId(int playerIndex);
    }
}