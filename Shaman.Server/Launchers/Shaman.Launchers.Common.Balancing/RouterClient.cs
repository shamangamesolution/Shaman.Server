using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Contract.Routing.Balancing;
using Shaman.Routing.Balancing.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Launchers.Common.Balancing
{
    public class RouterClient : IRouterClient
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly IRoutingConfig _routingConfig;
        private EntityDictionary<ServerInfo> _emptyList;

        public RouterClient(IRequestSender requestSender, IShamanLogger logger, IRoutingConfig routingConfig)
        {
            _requestSender = requestSender;
            _logger = logger;
            _routingConfig = routingConfig;
        }

        public async Task<EntityDictionary<ServerInfo>> GetServerInfoList(bool actualOnly)
        {
            var response = await _requestSender.SendRequest<GetServerInfoListResponse>(_routingConfig.RouterUrl,
                new GetServerInfoListRequest(actualOnly));
            if (response.ResultCode != ResultCode.OK)
            {
                _logger.Error(
                    $"BackendProvider error: error getting backends {response.ResultCode}|{response.Message}");
                _emptyList = new EntityDictionary<ServerInfo>();
                return _emptyList;
            }

            return response.ServerInfoList;
        }
    }
}