using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shaman.Common.Mvc;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Models;
using Shaman.Router.Data.Providers;
using Shaman.Routing.Balancing.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Router.Controllers
{
    public class ServerController : Controller
    {
        private static readonly DateTime FirstPingDate = DateTime.UtcNow;

        private readonly IRouterServerInfoProvider _serverInfoProvider;
        private readonly IShamanLogger _logger;
        private readonly IConfigurationRepository _configurationRepository;

        public ServerController(IRouterServerInfoProvider serverInfoProvider, IShamanLogger logger,
            IConfigurationRepository configurationRepository)
        {
            _serverInfoProvider = serverInfoProvider;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }

        [HttpGet]
        public JsonResult Ping()
        {
            _logger.Info($"Ping");
            return this.Json(new PingResult
            {
                ResultCode = 1,
                UtcNow = DateTime.UtcNow,
                UpDate = FirstPingDate
            });
        }

        [HttpPost]
        public async Task<ShamanResult> ActualizeServer(ActualizeServerOnRouterRequest request)
        {
            var response = new ActualizeServerOnRouterResponse();
            var serverInfoIdList = await _configurationRepository.GetServerId(request.ServerIdentity);

            if (serverInfoIdList == null || serverInfoIdList.Count == 0)
            {
                //create
                serverInfoIdList = new List<int>();
                var id = await _configurationRepository.CreateServerInfo(new ServerInfo(request.ServerIdentity,
                    request.Name,
                    request.Region, request.HttpPort, request.HttpsPort));
                serverInfoIdList.Add(id);
            }

            foreach (var item in serverInfoIdList)
                await _configurationRepository.UpdateServerInfoActualizedOn(item, request.PeersCount, request.HttpPort,
                    request.HttpsPort);

            return response;
        }

        [HttpPost]
        public async Task<ShamanResult> GetBundleUri(GetBundleUriRequest request)
        {
            var response = new GetBundleUriResponse();
            var serverInfoIdList = await _configurationRepository.GetServerId(request.ServerIdentity);
            if (serverInfoIdList == null || serverInfoIdList.Count == 0)
            {
                throw new Exception($"No server found with specified identity: {request.ServerIdentity}");
            }

            var bundleInfo = _serverInfoProvider.GetAllBundles()
                .SingleOrDefault(b => b.ServerId == serverInfoIdList.First());
            if (bundleInfo == null)
            {
                response.SetError("No bundles found");
            }
            else
            {
                response.BundleUri = bundleInfo.Uri;
            }

            return response;
        }

        [HttpPost]
        public async Task<ShamanResult> GetServerInfoList(GetServerInfoListRequest request)
        {
            var response = new GetServerInfoListResponse
            {
                ServerInfoList = new EntityDictionary<ServerInfo>(_serverInfoProvider.GetAllServers().Where(s =>
                    !request.ActualOnly || (request.ActualOnly && s.IsApproved && s.IsActual(10000))))
            };

            return response;
        }
    }
}