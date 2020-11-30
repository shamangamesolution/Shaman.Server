using System;
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
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;

namespace Sample.BackEnd.Controllers
{
    public class GamePlayController : WebControllerBase
    {
        
        public GamePlayController(IPlayerRepository playerRepo, ITempRepository tempRepo, IShamanLogger logger,
            IOptions<BackendConfiguration> config, ICacher cacher, IPlayerStorage playerStorage,
            ISerializer serializerFactory, IStorageContainer storageContainer) : base(playerRepo, tempRepo,
            logger, config, cacher, playerStorage, serializerFactory, storageContainer)
        {
        }
        
        [HttpPost]
        public async Task<ActionResult> GetPlayerGameData()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = SerializerFactory.DeserializeAs<GetPlayerGameDataRequest>(input);
            var response = new GetPlayerGameDataResponse();

            try
            {
                //validate request
                ValidateRequest(request);
                //init playerBl
                InitPlayerBL();
                var playerId = await ValidateToken(request.SessionId);
                response.Player = await PlayerStorage.GetPlayer(playerId, true);

            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"GetPlayerGameData: {ex}");

            }

            return new FileContentResult(SerializerFactory.Serialize(response), "text/html");
        }

    }
}