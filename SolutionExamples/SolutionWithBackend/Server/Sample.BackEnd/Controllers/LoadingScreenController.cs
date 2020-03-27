using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Caching;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.PlayerStorage;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.BackEnd.Extensions;
using Sample.Shared.Data.DTO.Requests;
using Sample.Shared.Data.DTO.Responses;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Storage;
using Sample.Shared.Extensions;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;

namespace Sample.BackEnd.Controllers
{
    public class LoadingScreenController : WebControllerBase
    {
        private IExternalAccountsRepository _externalAccountsRepo;
        private IShopRepository _shopRepository;
        
        public LoadingScreenController(IPlayerRepository playerRepo, 
            ITempRepository tempRepo, 
            IShamanLogger logger, 
            IOptions<BackendConfiguration> config, 
            ICacher cacher, 
            IPlayerStorage playerStorage,
            ISerializer serializerFactory,
            IStorageContainer storageContainer, IExternalAccountsRepository externalAccountsRepo, IShopRepository shopRepository) 
            : base(playerRepo, tempRepo, logger, config, cacher, playerStorage,serializerFactory, storageContainer)
        {
            _externalAccountsRepo = externalAccountsRepo;
            _shopRepository = shopRepository;
        }

        private async Task<InitializationResponse> InitLogic(InitializationRequest request, Player player, Guid sessionId)
        {
            var response = new InitializationResponse(player, sessionId);
  
            await PlayerStorage.UpdateLastOnlineDate(DateTime.UtcNow);

            return response;
        }

        private async Task<Player> GetPlayerByGuestId(string guestId)
        {
            int playerId = await PlayerRepo.GetPlayerIdByGuestId(guestId);
                
            Player player = null;
            //if finally it is = 0 - just create player
            if (playerId == 0)
            {
                player = await PlayerStorage.CreatePlayer(guestId);
                playerId = player.Id;
            }
            
            player = await PlayerStorage.GetPlayer(playerId, false);
            if (player == null)
                throw new Exception($"Player {playerId} was not found");

            return player;
        }

        private async Task<Player> GetPlayerByProvidersIds(Dictionary<int, string> providers)
        {
            Player player = null;

            foreach (var provider in providers)
            {
                var externalAccount = await _externalAccountsRepo.GetExternalAccounts(provider.Key, provider.Value);
                if (!externalAccount.IsNullOrEmpty())
                {
                    var acc = externalAccount.FirstOrDefault();
                    if (acc == null)
                        continue;
                    //this player could be deleted while DB cleaning
                    if (await PlayerRepo.GetPlayerIdByGuestId(acc.GuestId) == 0)
                        continue;
                    
                    player = await PlayerStorage.GetPlayer(acc.PlayerId, false);

                    return player;
                }
            }

            return null;
        }
        
        [HttpPost]
        public async Task<ActionResult> Initialization()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = SerializerFactory.DeserializeAs<InitializationRequest>(input);
            var response = new InitializationResponse();

            try
            {
                var guestId = request.GuestId;
                var authToken = request.AuthToken;
                
                //check guest token
                if (!(await Cacher.ValidateAuthToken(authToken)))
                    throw new Exception($"AuthToken is not valid");

                //validate request
                ValidateRequest(request);
                
                //init playerBl
                InitPlayerBL();
                
                Player player = null;
                //get player based on input parameter - using providers ID or guestId
                if (request.ProviderIds == null || !Enumerable.Any<KeyValuePair<int, string>>(request.ProviderIds))
                {
                    player = await GetPlayerByProvidersIds(request.ProviderIds) ?? await GetPlayerByGuestId(request.GuestId);
                }
                else
                    player = await GetPlayerByGuestId(request.GuestId);

                var sessionId = await Cacher.CreateToken(player.Id);
                
                response = await InitLogic(request, player, sessionId);

                Cacher.Put(player);
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"Initialization error: {ex}");
            }
            
            return new FileContentResult(SerializerFactory.Serialize(response), "text/html");
        }



    }
}