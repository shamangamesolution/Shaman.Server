using System.Threading.Tasks;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Entity.Currency;
using Shaman.Messages;

namespace Sample.BackEnd.Data.Repositories.Interfaces
{
    public interface IPlayerRepository
    {

        Task<Player> GetPlayerInfo(int playerId);       
        Task ChangeName(int playerId, string newName);
        Task UpdateLastOnlineDate(int playerId);
        Task UpdateLevel(int playerId, byte newLevel);
        //social and fireteams
        
        Task<int> GetPlayerIdByGuestId(string guestId);
        Task<Player> CreatePlayer(string guestId);

        Task<int> CreateWalletItem(PlayerWalletItem item);

        Task<EntityDictionary<PlayerWalletItem>> GetWalletItems(int playerId);

        Task UpdateWalletItem(int playerWalletItemId, uint quantity);
    }
}
