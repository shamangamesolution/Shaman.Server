using System.Threading.Tasks;
using Sample.Shared.Data.Entity.ExternalAccounts;
using Shaman.Messages;

namespace Sample.BackEnd.Data.Repositories.Interfaces
{
    public interface IExternalAccountsRepository
    {
        Task<EntityDictionary<ExternalAccount>> GetExternalAccounts(int authProviderId, string externalId);
        Task<string> GetGuestId(int id);
        Task CreateExternalAccount(ExternalAccount externalAccount);
        Task UnlinkAccount(int authProviderId, int playerId);
    }
}