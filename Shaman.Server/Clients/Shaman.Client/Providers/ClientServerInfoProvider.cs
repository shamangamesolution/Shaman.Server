using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Contract.Routing.Balancing;
using Shaman.Messages.General.Entity;
using Shaman.Serialization.Messages;

namespace Shaman.Client.Providers
{
    public class ClientServerInfoProvider : IClientServerInfoProvider
    {
        private readonly IShamanLogger _logger;
        private IRouterClient _routerClient;
        
        public ClientServerInfoProvider(IShamanLogger logger, IRouterClient routerClient)
        {
            _logger = logger;
            _routerClient = routerClient;
        }

        public async Task GetRoutes(string routerUrl, string clientVersion, Action<List<Route>> callback)
        {
            var response = await _routerClient.GetServerInfoList(false);
            callback(BuildRoutes(clientVersion, response));
        }

        public virtual async Task<List<Route>> GetRoutes(string routerUrl, string clientVersion)
        {
            var list = await _routerClient.GetServerInfoList(false);

            // var response = await _requestSender.SendRequest<GetServerInfoListResponse>(routerUrl, new GetServerInfoListRequest(actualOnly: false));
            if (list == null)
                throw new Exception($"Error requesting routes");
            return BuildRoutes(clientVersion, list);
        }

        protected List<Route> BuildRoutes(string clientVersion, EntityDictionary<ServerInfo> serverInfoList)
        {
            var result = new List<Route>();
            if (serverInfoList == null || !serverInfoList.Any())
                return result;
            
            var servers = serverInfoList.Where(s => s.ClientVersionList.Contains(clientVersion) && s.IsApproved).ToList();

            var regions = servers.Select(s => s.Region).Distinct();
            foreach (var region in regions)
            {
                //ping host is game server in this region
                var pingHost = servers.FirstOrDefault(s =>
                    s.ServerRole == ServerRole.GameServer && s.Region == region);

                //get matchmaker
                var matchMaker = servers.FirstOrDefault(s =>
                    s.ServerRole == ServerRole.MatchMaker && s.Region == region);

                //get backend
                var backEnd = servers.FirstOrDefault(s =>
                    s.ServerRole == ServerRole.BackEnd && s.Region == region);

                if (pingHost != null && matchMaker != null && backEnd != null)
                {
                    var protocol = backEnd.HttpsPort > 0 ? "https" : "http";
                    var port = backEnd.HttpsPort > 0 ? backEnd.HttpsPort : backEnd.HttpPort;

                    result.Add(new Route(region, matchMaker.Name, pingHost.Address, protocol, backEnd.Address, port,
                        backEnd.Id, matchMaker.Address, matchMaker.GetLessLoadedPort()));
                }
            }
            

            return result;
        }
    }
}