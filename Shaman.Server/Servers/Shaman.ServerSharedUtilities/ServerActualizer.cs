using System.Threading.Tasks;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Servers;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;

namespace Shaman.ServerSharedUtilities
{
    public class ServerActualizer : IServerActualizer
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        
        private readonly IApplicationConfig _config;


        public ServerActualizer(IRequestSender requestSender, IApplicationConfig config, IShamanLogger logger)
        {
            _requestSender = requestSender;
            _config = config;
            _logger = logger;
        }

        public async Task Actualize(int peersCount, ushort httpPort = 0, ushort httpsPort = 0)
        {
            await _requestSender.SendRequest<ActualizeServerOnRouterResponse>(_config.GetRouterUrl(),
                new ActualizeServerOnRouterRequest(GetServerIdentity(), _config.GetServerName(), _config.GetRegion(), peersCount, httpPort, httpsPort),
                (response) =>
                {
                    if (!response.Success)
                    {
                        _logger.Error($"MatchMakerServerInfoProvider.ActualizeMe error: {response.Message}");
                    }
                });
        }
        
        private ServerIdentity GetServerIdentity()
        {
            return new ServerIdentity(_config.GetPublicName(),
                _config.GetListenPorts(), _config.GetServerRole());
        }


    }
}