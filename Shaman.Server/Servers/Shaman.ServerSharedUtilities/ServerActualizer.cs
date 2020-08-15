using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Messages;

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

        public async Task Actualize(int peersCount)
        {
            await _requestSender.SendRequest<ActualizeServerOnRouterResponse>(_config.GetRouterUrl(),
                new ActualizeServerOnRouterRequest(GetServerIdentity(), _config.GetServerName(), _config.GetRegion(), peersCount, _config.BindToPortHttp),
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