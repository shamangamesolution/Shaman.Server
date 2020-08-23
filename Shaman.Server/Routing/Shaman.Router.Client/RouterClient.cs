using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Common.Server.Messages;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Router.Client
{
    public interface IRouterClient
    {
        Task<EntityDictionary<ServerInfo>> GetServerInfoList();
        Task Actualize(ServerIdentity identity, string serverName, string region, int peersCount, ushort httpPort);
    }

    public class RouterClient : IRouterClient
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly RouterConfig _routerConfig;
        private EntityDictionary<ServerInfo> _emptyList;

        public RouterClient(IRequestSender requestSender, IShamanLogger logger, RouterConfig routerConfig)
        {
            _requestSender = requestSender;
            _logger = logger;
            _routerConfig = routerConfig;
        }

        public async Task<EntityDictionary<ServerInfo>> GetServerInfoList()
        {
            var response = await _requestSender.SendRequest<GetServerInfoListResponse>(_routerConfig.RouterUrl,
                new GetServerInfoListRequest());
            if (response.ResultCode != ResultCode.OK)
            {
                _logger.Error(
                    $"BackendProvider error: error getting backends {response.ResultCode}|{response.Message}");
                _emptyList = new EntityDictionary<ServerInfo>();
                return _emptyList;
            }

            return response.ServerInfoList;
        }

        public async Task Actualize(ServerIdentity identity, string serverName, string region, int peersCount, ushort httpPort)
        {
            var response = await _requestSender.SendRequest<ActualizeServerOnRouterResponse>(_routerConfig.RouterUrl,
                new ActualizeServerOnRouterRequest(identity, serverName, region, peersCount, httpPort));
            if (!response.Success)
            {
                _logger.Error($"MatchMakerServerInfoProvider.ActualizeMe error: {response.Message}");
            }
        }
    }
}