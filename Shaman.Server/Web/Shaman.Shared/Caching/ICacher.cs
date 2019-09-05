using System;
using System.Threading.Tasks;
using Shaman.Messages.General.Entity;

namespace Shaman.Shared.Caching
{
    public interface ICacher
    {
        Task Put(Player player);
        Task<Player> Get(int playerId);
        Task<Guid> CreateToken(int playerId);
        Task<int> GetPlayerId(Guid token);
        Task RemoveFromCache(int playerId);
        Task<Guid> GetAuthToken();
        Task<bool> ValidateAuthToken(Guid token);
    }
}