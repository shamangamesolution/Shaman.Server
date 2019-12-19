using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Shaman.Common.Utils.Logging;
using Shaman.DAL.Repositories;

namespace Sample.BackEnd.Data.Repositories
{
    public class ShopRepository : RepositoryBase, IShopRepository
    {
        public ShopRepository(IOptions<BackendConfiguration> config, IShamanLogger logger)
        {
            Initialize(config.Value.DbServerTemp, config.Value.DbNameTemp, config.Value.DbUserTemp, config.Value.DbPasswordTemp, config.Value.DbMaxPoolSize, logger);
        }

        
        public async Task<bool> IsTransactionExists(string vendorReceipt, int playerId)
        {
            return false;
        }
    }
}