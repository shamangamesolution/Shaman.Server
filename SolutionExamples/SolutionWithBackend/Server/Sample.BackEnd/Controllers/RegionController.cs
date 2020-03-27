using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Caching;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.PlayerStorage;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.BackEnd.Extensions;
using Sample.BackEnd.Models;
using Sample.Shared.Data.DTO.Requests;
using Sample.Shared.Data.DTO.Responses;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Requests.Auth;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.General.DTO.Responses.Auth;
using Shaman.Messages.General.Entity;

namespace Sample.BackEnd.Controllers
{
    public class RegionController : WebControllerBase
    {
        private Random _rnd = new Random();
        private IParametersRepository _paramsRepo;
        
        public RegionController( 
            IPlayerRepository playerRepo, 
            ITempRepository tempRepo,
            IShamanLogger logger, 
            IOptions<BackendConfiguration> config,
            ICacher cacher,
            IParametersRepository paramsRepo,
            IPlayerStorage playerStorage,
            ISerializer serializerFactory,
            IStorageContainer storageContainer) 
            : base(playerRepo, tempRepo, logger, config, cacher, playerStorage, serializerFactory, storageContainer)
        {
            _paramsRepo = paramsRepo;
        }

        [HttpGet]
        public async Task<JsonResult> Ping()
        {
            var resultErrorInput = new Result { ResultCode = 1 };
            JsonResult resultErrorInputJson = this.Json(resultErrorInput);
            return resultErrorInputJson;
        }

        [HttpPost]
        public async Task<ActionResult> IsOnService()
        {            
            var input = await Request.GetRawBodyBytesAsync(); 

            var getUserRequest = SerializerFactory.DeserializeAs<IsOnServiceRequest>(input);

            var response = new IsOnServiceResponse();

            try
            {
                var id = Guid.NewGuid().ToString();

                response.IsOnService = await _paramsRepo.GetBoolValue(ParameterNames.IsOnService);
                response.ClientVersion = "";//_versionRepo.GetVersion(VersionType.Client).ToString();

            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"IsOnService error: {ex}");
            }

            return new FileContentResult(SerializerFactory.Serialize(response), "text/html");
        }

        [HttpPost]
        public async Task<ActionResult> GetAuthToken()
        {
            var input = await Request.GetRawBodyBytesAsync(); 

            var getUserRequest = SerializerFactory.DeserializeAs<GetAuthTokenRequest>(input);

            var response = new GetAuthTokenResponse();
            
            try
            {
                response.AuthToken = await Cacher.GetAuthToken();
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"GetAuthToken error: {ex}");
            }

            return new FileContentResult(SerializerFactory.Serialize(response), "text/html");
        }

        [HttpPost]
        public async Task<ActionResult> ValidateSessionId()
        {
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = SerializerFactory.DeserializeAs<ValidateSessionIdRequest>(input);

            var response = new ValidateSessionIdResponse();

            try
            {
                if (request.Secret != Config.Value.CustomSecret)
                    throw new Exception("General auth error");
                
                var playerId = await ValidateToken(request.SessionId);
                
                if (playerId == 0)
                    throw new Exception($"Auth error: sessionId {request.SessionId} is not valid");
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                response.ResultCode = ResultCode.NotAuthorized;
                LogError($"ValidateSessionId error: {ex}");
            }

            return new FileContentResult(SerializerFactory.Serialize(response), "text/html");
        }
        
        [HttpPost]
        public async Task<ActionResult> GetStorageVersion()
        {
            var input = await Request.GetRawBodyBytesAsync(); 

            var request = SerializerFactory.DeserializeAs<GetCurrentStorageVersionRequest>(input);

            var response = new GetCurrentStorageVersionResponse();

            try
            {
                response.CurrentDatabaseVersion =
                    StorageContainer.GetStorage()
                        .DatabaseVersion; //_versionRepo.GetVersion(VersionType.DataBase).ToString();

                response.CurrentBackendVersion = Config.Value.ServerVersion;
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"GetStorageVersion error: {ex}");
            }

            return new FileContentResult(SerializerFactory.Serialize(response), "text/html");
        }
    }
}