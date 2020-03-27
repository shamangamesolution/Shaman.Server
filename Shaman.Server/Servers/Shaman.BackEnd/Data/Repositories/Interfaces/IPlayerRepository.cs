using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Messages.General.Entity.Wallet;

namespace Shaman.BackEnd.Data.Repositories.Interfaces
{
    public interface IPlayerRepository
    {

        Task<Player> GetPlayerInfo(int playerId, SerializationRules serializationRules = SerializationRules.AllInfo);       
        Task ChangeName(int playerId, string newName);
        Task UpdateLastOnlineDate(int playerId);
        Task UpdateLevel(int playerId, byte newLevel);
        //social and fireteams
        
        Task<int> GetPlayerIdByGuestId(string guestId);
        Task<Player> CreatePlayer(string guestId);


        Task<int> CreateWalletItem(PlayerWalletItem item);

        Task<List<PlayerWalletItem>> GetWalletItems(int playerId);

        Task UpdateWalletItem(int playerWalletItemId, uint quantity);
    }
}
