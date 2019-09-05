using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Models;
using Shaman.Shared.Extensions;
using Shaman.Messages.MM;

namespace Shaman.Router.Controllers
{
    public class ServerController : WebControllerBase
    {
        public ServerController(IConfigurationRepository configRepo, IShamanLogger logger, IOptions<RouterConfiguration> config, ISerializerFactory serializerFactory) 
            : base(configRepo, logger, config, serializerFactory)
        {
        }

        [HttpGet]
        public async Task<JsonResult> Ping()
        {
            LogInfo($"Ping");

            var resultErrorInput = new Result { ResultCode = 1 };
            JsonResult resultErrorInputJson = this.Json(resultErrorInput);
            return resultErrorInputJson;
        }
        
        [HttpPost]
        public async Task<ActionResult> GetMatchmakers()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = MessageBase.DeserializeAs<GetMatchmakersRequest>(SerializerFactory, input);//GetServerConfigurationsRequest.Deserialize(input);
            var response = new GetMatchmakersResponse();
            
            try
            {
                LogInfo($"Matchmakers requested: {request.ClientVersion}");
                //LogInfo("GetServerConfigurations2", $"Get GetConfigsRequest: auth = {getConfigsRequest.AuthCode} version = {getConfigsRequest.ClientVersion} editorId = {getConfigsRequest.EditorID} googleId = {getConfigsRequest.PlayerGooglePlayId}");
                string version = request.ClientVersion;

                var configs = await ConfigRepo.GetAllMmConfigurations(request.Game);               

                if (!string.IsNullOrEmpty(version))
                    configs = configs.Where(c => c.Version == request.ClientVersion).ToList();
                
                var backends = await ConfigRepo.GetBackends();
                foreach (var config in configs)
                {
                    var backend = backends.FirstOrDefault(b => b.Id == config.BackendId);
                    if (backend == null)
                    {
                        Logger.Error($"Mm config {config.Id} references unavailable backend {config.BackendId}");
                        continue;
                    }

                    config.BackendAddress = backend.Address;
                    config.BackendPort = backend.Port;
                }
                
                //set response
                response.Matchmakers = configs;
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"{ex.ToString()}");
            }

            return new FileContentResult(response.Serialize(SerializerFactory), "text/html");
        }
        
        [HttpPost]
        public async Task<ActionResult> ActualizeMatchMaker()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = MessageBase.DeserializeAs<ActualizeMatchMakerRequest>(SerializerFactory, input);//ActualizeServerRequest.Deserialize(input) as ActualizeServerRequest;
            var response = new ActualizeMatchMakerResponse();

            try
            {
                //check secret
                if (Config.Value.CustomSecret != request.Secret)
                    throw new Exception($"Secret is not valid");
//                
                var configs = await ConfigRepo.GetAllMmConfigurations(request.GameProject, false);
                if (configs.Any(c => c.Address == request.IpAddress && c.Port == request.Port))
                {
                    //update config
                    //ConfigRepo.UpdateConfiguration(request.Game, request.ServerName, request.MasterPeers, request.GamePeers);
                    ConfigRepo.UpdateMmConfiguration(request.GameProject, request.Name,
                        request.IpAddress, request.Port);
                }
                else
                {
                    //create config
//                    ConfigRepo.CreateConfiguration(request.Game, request.ServerName, "", "0", request.Region, request.MasterPeers,
//                        request.GamePeers);
                    ConfigRepo.CreateMmConfiguration(request.GameProject, request.Name,
                        request.IpAddress, request.Port);
                }
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"{ex.ToString()}");
            }

            return new FileContentResult(response.Serialize(SerializerFactory), "text/html");
        }
        
        [HttpPost]
        public async Task<ActionResult> ActualizeServer()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = MessageBase.DeserializeAs<ActualizeServerRequest>(SerializerFactory, input);//ActualizeServerRequest.Deserialize(input) as ActualizeServerRequest;
            var response = new ActualizeServerResponse();
            
            try
            {
                //check secret
//                if (Config.Value.CustomSecret != request.Secret)
//                    throw new Exception($"Secret {request.Secret} is not valid");
//                
//                var configs = await ConfigRepo.GetAllConfigurations(request.Game, false);
//                if (configs.Any(c => c.ServerName == request.ServerName))
//                {
//                    //update config
//                    ConfigRepo.UpdateConfiguration(request.Game, request.ServerName, request.MasterPeers, request.GamePeers);
//                }
//                else
//                {
//                    //create config
//                    ConfigRepo.CreateConfiguration(request.Game, request.ServerName, "", "0", request.Region, request.MasterPeers,
//                        request.GamePeers);
//                }
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"{ex.ToString()}");
            }

            return new FileContentResult(response.Serialize(SerializerFactory), "text/html");
        }
   
        [HttpPost]
        public async Task<ActionResult> GetBackendsList()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = MessageBase.DeserializeAs<GetBackendsListRequest>(SerializerFactory, input);//ActualizeServerRequest.Deserialize(input) as ActualizeServerRequest;
            var response = new GetBackendsListResponse();
            
            try
            {
                response.Backends = await ConfigRepo.GetBackends();
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"{ex.ToString()}");
            }

            return new FileContentResult(response.Serialize(SerializerFactory), "text/html");
        }
    }
}