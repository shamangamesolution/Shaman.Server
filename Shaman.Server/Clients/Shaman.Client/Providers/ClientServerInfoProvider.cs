using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.Messages.General.Entity.Router;

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
                    var result = new List<Route>();

                    if (!response.Success)
                    {
                        _logger.Error($"ClientServerInfoProvider.GetRoutes response error: {response.Message}");
                        callback(result);
                        return;
                    }

                    var servers = response.ServerInfoList.Where(s => s.ClientVersion == clientVersion).ToList();

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
                            
                            result.Add(new Route(region, pingHost.Address, protocol, backEnd.Address, port,backEnd.Id, matchMaker.Address, matchMaker.GetLessLoadedPort()));
                        }
                    }

                    //return result
                    callback(result);
                });
        }
    }
}