using System.Threading.Tasks;
using Shaman.Client;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Contract.Routing.Balancing;
using Shaman.Routing.Balancing.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.TestTools.ClientPeers
{
    public class TestRouterClient : IRouterClient
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private EntityDictionary<ServerInfo> _emptyList;
        private string _routerUrl;

        public TestRouterClient(IRequestSender requestSender, IShamanLogger logger, string routerUrl)
        {
            _requestSender = requestSender;
            _logger = logger;
            _routerUrl = routerUrl;
        }

        public async Task<EntityDictionary<ServerInfo>> GetServerInfoList(bool actualOnly)
        {
            var response = await _requestSender.SendRequest<GetServerInfoListResponse>(_routerUrl,
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