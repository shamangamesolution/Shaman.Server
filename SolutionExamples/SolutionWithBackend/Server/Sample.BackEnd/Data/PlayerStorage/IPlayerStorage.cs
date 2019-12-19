using System;
using System.Threading.Tasks;
using Sample.Shared.Data.Entity;

namespace Sample.BackEnd.Data.PlayerStorage
{
    public interface IPlayerStorage
    {
        Task<Player> CreatePlayer(string guestId);
        Task<Player> GetPlayer();
        Task<Player> GetPlayer(int playerId, bool useCache);
        Task PutPlayerToCache();
        Task UpdateWalletBalance(int currency, int valueToAdd);
        Task AddCurrencyValue(int currencyId, int valueToAdd);        
        Task RemoveCurrencyValue(int currencyId, int valueToRemove);
        Task UpdateLastOnlineDate(DateTime dateTime);
    }
}