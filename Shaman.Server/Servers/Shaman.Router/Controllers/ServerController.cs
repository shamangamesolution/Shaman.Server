using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.Messages.General.Entity.Router;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Models;
using Shaman.Messages.MM;
using Shaman.Router.Data.Providers;
using Shaman.ServerSharedUtilities.Extensions;

namespace Shaman.Router.Controllers
{
    public class ServerController : WebControllerBase
    {
        private IRouterServerInfoProvider _serverInfoProvider;
        public ServerController(IConfigurationRepository configRepo, IShamanLogger logger, IOptions<RouterConfiguration> config, ISerializer serializer, IRouterServerInfoProvider serverInfoProvider) 
            : base(configRepo, logger, config, serializer)
        {
            _serverInfoProvider = serverInfoProvider;
        }

        [HttpGet]
        public JsonResult Ping()
        {
            LogInfo($"Ping");

            return this.Json(new Result { ResultCode = 1 });
        }
        
        [HttpPost]
        public async Task<ActionResult> ActualizeServer()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = Serializer.DeserializeAs<ActualizeServerOnRouterRequest>(input);//ActualizeServerRequest.Deserialize(input) as ActualizeServerRequest;
            var response = new ActualizeServerOnRouterResponse();
            
            try
            {
                var serverInfoId = await ConfigRepo.GetServerId(request.ServerIdentity);
                if (serverInfoId == null)
                {
                    //create
                    serverInfoId = await ConfigRepo.CreateServerInfo(new ServerInfo(request.ServerIdentity, request.Name, request.Region, request.HttpPort, request.HttpsPort));
                }
                await ConfigRepo.UpdateServerInfoActualizedOn(serverInfoId.Value, request.PeersCount, request.Name, request.Region, request.HttpPort, request.HttpsPort);
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"{ex}");
            }

            return new FileContentResult(Serializer.Serialize(response), "text/html");
        }

        [HttpPost]
        public async Task<ActionResult> GetBundleUri()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = Serializer.DeserializeAs<GetBundleUriRequest>(input);
            var response = new GetBundleUriResponse();

            try
            {
                var serverInfoId = await ConfigRepo.GetServerId(request.ServerIdentity);
                if (!serverInfoId.HasValue)
                {
                    throw new Exception($"No server found with specified identity: {request.ServerIdentity}");
                }

                var bundleInfo = _serverInfoProvider.GetAllBundles().Single(b => b.ServerId == serverInfoId.Value);
                response.BundleUri = bundleInfo.Uri;
            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"{ex}");
            }

            return new FileContentResult(Serializer.Serialize(response), "text/html");
        }
   
        [HttpPost]
        public async Task<ActionResult> GetServerInfoList()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = Serializer.DeserializeAs<GetServerInfoListRequest>(input);
            var response = new GetServerInfoListResponse();
            
            try
            {
                response.ServerInfoList = new EntityDictionary<ServerInfo>(_serverInfoProvider.GetAllServers().Where(s =>
                    !request.ActualOnly || (request.ActualOnly && s.IsApproved && s.IsActual(10000))));

            }
            catch (Exception ex)
            {
                response.SetError(ex.Message);
                LogError($"{ex.ToString()}");
            }

            return new FileContentResult(Serializer.Serialize(response), "text/html");
        }
    }
}