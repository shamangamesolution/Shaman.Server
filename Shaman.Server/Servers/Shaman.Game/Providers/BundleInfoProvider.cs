using System;
using System.Threading.Tasks;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Servers;
using Shaman.Game.Configuration;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;

namespace Shaman.Game.Providers
{
    public class BundleInfoProvider : IBundleInfoProvider
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly GameApplicationConfig _config;

        public BundleInfoProvider(IRequestSender requestSender, IApplicationConfig config, IShamanLogger logger)
        {
            _requestSender = requestSender;
            _logger = logger;
            _config = (GameApplicationConfig) config;
        }

        private ServerIdentity GetServerIdentity()
        {
            return new ServerIdentity(_config.GetPublicName(),
                _config.GetListenPorts(), _config.GetServerRole());
        }

        public async Task<string> GetBundleUri()
        {
            var serverIdentity = GetServerIdentity();
            var response = await _requestSender.SendRequest<GetBundleUriResponse>(_config.GetRouterUrl(),
                new GetBundleUriRequest(serverIdentity));

            if (!response.Success)
            {
                _logger.Error($"MatchMakerServerInfoProvider.GetBundleUri error: {response.Message}");
                throw new BundleNotFoundException($"Bundle not found for: {serverIdentity}");
            }

            return response.BundleUri;
        }
    }

    public class BundleNotFoundException : Exception
    {
        public BundleNotFoundException(string msg) : base(msg)
        {
        }
    }
}