using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Client.Providers
{
    public class ClientServerInfoProvider : IClientServerInfoProvider
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        
        public ClientServerInfoProvider(IRequestSender requestSender, IShamanLogger logger)
        {
            _requestSender = requestSender;
            _logger = logger;
        }

        public Task GetRoutes(string routerUrl, string clientVersion, Action<List<Route>> callback)
        {
            return _requestSender.SendRequest<GetServerInfoListResponse>(routerUrl, new GetServerInfoListRequest(actualOnly: false),
                (response) =>
                {
                    callback(BuildRoutes(clientVersion, response));
                });
        }

        public virtual async Task<List<Route>> GetRoutes(string routerUrl, string clientVersion)
        {
            var response = await _requestSender.SendRequest<GetServerInfoListResponse>(routerUrl, new GetServerInfoListRequest(actualOnly: false));
            if (!response.Success && response.ResultCode == ResultCode.SendRequestError)
                throw new Exception($"Error requesting routes");
            return BuildRoutes(clientVersion, response);
        }

        protected List<Route> BuildRoutes(string clientVersion, GetServerInfoListResponse response)
        {
            var result = new List<Route>();

            if (!response.Success)
            {
                _logger.Error($"ClientServerInfoProvider.GetRoutes response error: {response.Message}");
            }
            else
            {
                var servers = response.ServerInfoList.Where(s => s.ClientVersion == clientVersion && s.IsApproved).ToList();

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
            }

            return result;
        }
    }
}