using System;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Bundling.Common;
using Shaman.Common.Http;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Routing.Balancing.Messages;

namespace Shaman.Launchers.Common.Balancing
{
    public class RouterBundleInfoProvider : IBundleInfoProvider
    {
        private const int BundleRetryMsec = 1500;
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly IBalancingBundleInfoProviderConfig _config;

        public RouterBundleInfoProvider(IRequestSender requestSender, IBalancingBundleInfoProviderConfig config, IShamanLogger logger)
        {
            _requestSender = requestSender;
            _logger = logger;
            _config = config;
        }

        private ServerIdentity GetServerIdentity()
        {
            return new ServerIdentity(_config.PublicName,
                _config.Ports, _config.Role);
        }

        public async Task<bool> GetToOverwriteExisting()
        {
            return _config.OverwriteBundle;
        }

        public async Task<string> GetServerRole()
        {
            return _config.Role.ToString();
        }

        public async Task<string> GetBundleUri()
        {
            var serverIdentity = GetServerIdentity();
            var response = await _requestSender.SendRequest<GetBundleUriResponse>(_config.RouterUrl,
                new GetBundleUriRequest(serverIdentity));

            if (!response.Success)
            {
                throw new BundleNotFoundException($"Bundle not found for: {serverIdentity}, requested from '{_config.RouterUrl}': {response.Message}");
            }

            _logger.Error($"Bundle uri received for '{serverIdentity}' (overwrite = {_config.OverwriteBundle}): {response.BundleUri}");
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