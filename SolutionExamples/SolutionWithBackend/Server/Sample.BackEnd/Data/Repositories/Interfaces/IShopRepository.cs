using System.Threading.Tasks;

namespace Sample.BackEnd.Data.Repositories.Interfaces
{
    public interface IShopRepository
    {
        Task<bool> IsTransactionExists(string vendorReceipt, int playerId);
    }
}