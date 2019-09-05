using System;
using System.Threading.Tasks;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;

namespace Shaman.BackEnd.Data.PlayerStorage
{
    public interface IPlayerStorage
    {
        void SetStorage(DataStorage storage);
        Task<Player> GetPlayer();
        Task<Player> GetPlayer(int playerId, bool useCache, SerializationRules serializaionRules = SerializationRules.AllInfo);
        Task PutPlayerToCache();
        Task GiveLevel(byte newLevel);
        Task ChangeName(string newName);

        Task UpdateWalletBalance(int currency, int valueToAdd);
        Task AddCurrencyValue(int currencyId, uint valueToAdd);        
        Task RemoveCurrencyValue(int currencyId, uint valueToRemove);
        Task UpdateLastOnlineDate(DateTime dateTime);
    }
}