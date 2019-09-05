using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shaman.BackEnd.Config;
using Shaman.BackEnd.Data.PlayerStorage;
using Shaman.BackEnd.Data.Repositories.Interfaces;
using Shaman.Common.Utils.Extensions;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Shared.Caching;
using Shaman.Shared.Extensions;

namespace Shaman.BackEnd.Controllers
{
    public class LoadingScreenController : WebControllerBase
    {
        public LoadingScreenController(IPlayerRepository playerRepo, 
            ITempRepository tempRepo, 
            IShamanLogger logger, 
            IOptions<BackendConfiguration> config, 
            ICacher cacher, 
            IPlayerStorage playerStorage,
            ISerializerFactory serializerFactory,
            IStorageContainer storageContainer) 
            : base(playerRepo, tempRepo, logger, config, cacher, playerStorage,serializerFactory, storageContainer)
        {

        }

        private async Task<InitializationResponse> InitLogic(InitializationRequest request, Player player, Guid sessionId)
        {
            var response = new InitializationResponse(SerializationRules.AllInfo, player, sessionId);
  
            await PlayerStorage.UpdateLastOnlineDate(DateTime.UtcNow);

            return response;
        }

        [HttpPost]
        public async Task<ActionResult> Initialization()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = MessageBase.DeserializeAs<InitializationRequest>(SerializerFactory, input);//ActualizeServerRequest.Deserialize(input) as ActualizeServerRequest;
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
                
                int playerId = await PlayerRepo.GetPlayerIdByGuestId(guestId);
                
                Player player = null;
                //if finally it is = 0 - just create player
                if (playerId == 0)
                {
                    player = await PlayerRepo.CreatePlayer(guestId);
                    playerId = player.Id;
                }
                
                var sessionId = await Cacher.CreateToken(playerId);
                
                player = await PlayerStorage.GetPlayer(playerId, false);
                if (player == null)
                    throw new Exception($"Player {playerId} was not found");

                response = await InitLogic(request, player, sessionId);
                response.SerializationRules = SerializationRules.AllInfo;
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"Initialization error: {ex}");
            }
            
            return new FileContentResult(response.Serialize(SerializerFactory), "text/html");
        }


    }
}