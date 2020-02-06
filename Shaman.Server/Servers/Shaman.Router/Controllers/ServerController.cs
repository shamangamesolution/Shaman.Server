using System;
using System.Collections.Generic;
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
        private static DateTime _firstPingDate = DateTime.UtcNow;
        
        private readonly IRouterServerInfoProvider _serverInfoProvider;
        public ServerController(IConfigurationRepository configRepo, IShamanLogger logger, IOptions<RouterConfiguration> config, ISerializer serializer, IRouterServerInfoProvider serverInfoProvider) 
            : base(configRepo, logger, config, serializer)
        {
            _serverInfoProvider = serverInfoProvider;
        }

        [HttpGet]
        public JsonResult Ping()
        {
            LogInfo($"Ping");

            return this.Json(new PingResult
            {
                ResultCode = 1,
                UtcNow = DateTime.UtcNow,
                UpDate = _firstPingDate
            });
        }
        
        [HttpPost]
        public async Task<ActionResult> ActualizeServer()
        {
            var input = await Request.GetRawBodyBytesAsync();

            var request = Serializer.DeserializeAs<ActualizeServerOnRouterRequest>(input);//ActualizeServerRequest.Deserialize(input) as ActualizeServerRequest;
            var response = new ActualizeServerOnRouterResponse();
            
            try
            {
                var serverInfoIdList = await ConfigRepo.GetServerId(request.ServerIdentity);
                
                if (serverInfoIdList == null || serverInfoIdList.Count == 0)
                {
                    //create
                    serverInfoIdList = new List<int>();
                    var id = await ConfigRepo.CreateServerInfo(new ServerInfo(request.ServerIdentity, request.Name, request.Region, request.HttpPort, request.HttpsPort));
                    serverInfoIdList.Add(id);
                }
                foreach(var item in serverInfoIdList)
                    await ConfigRepo.UpdateServerInfoActualizedOn(item, request.PeersCount, request.Name, request.Region, request.HttpPort, request.HttpsPort);
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
                var serverInfoIdList = await ConfigRepo.GetServerId(request.ServerIdentity);
                if (serverInfoIdList == null || serverInfoIdList.Count == 0)
                {
                    throw new Exception($"No server found with specified identity: {request.ServerIdentity}");
                }

                var bundleInfo = _serverInfoProvider.GetAllBundles().Single(b => b.ServerId == serverInfoIdList.First());
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