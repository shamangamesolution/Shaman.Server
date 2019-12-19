using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Caching;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.PlayerStorage;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.Shared.Data.DTO.Requests;
using Sample.Shared.Data.DTO.Responses;
using Sample.Shared.Data.Entity.ExternalAccounts;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;

namespace Sample.BackEnd.Controllers
{
    public class MainController : WebControllerBase
    {
        private IExternalAccountsRepository _externalAccountsRepo;

        public MainController(IPlayerRepository playerRepo, 
            ITempRepository tempRepo, 
            IShamanLogger logger, 
            IOptions<BackendConfiguration> config, 
            ICacher cacher, 
            IPlayerStorage playerStorage,
            ISerializer serializerFactory,
            IStorageContainer storageContainer, IExternalAccountsRepository externalAccountsRepo) 
            : base(playerRepo, tempRepo, logger, config, cacher, playerStorage,serializerFactory, storageContainer)
        {
            _externalAccountsRepo = externalAccountsRepo;
        }
        
        [HttpPost]
        public async Task<ActionResult> LinkExternalAccount()
        {
            return await ProcessRequest<LinkExternalAccountRequest, LinkExternalAccountResponse>(async (request, response, player) =>
            {
                await _externalAccountsRepo.CreateExternalAccount(new ExternalAccount(request.ProviderId, player.Id,
                    request.ExternalId, player.GuestId));
            });            
        }
    }
}