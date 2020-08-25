using System;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Bundling.Common;
using Shaman.Common.Http;
using Shaman.Common.Server.Messages;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Messages;
using Shaman.Routing.Common.Actualization;

namespace Shaman.Bundling.Balancing
{
    public class BundleInfoProvider : IBundleInfoProvider
    {
        private const int BundleRetryMsec = 1500;
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly IBundleInfoProviderConfig _config;

        public BundleInfoProvider(IRequestSender requestSender, IBundleInfoProviderConfig config, IShamanLogger logger)
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

        public async Task<string> GetBundleUri()
        {
            var messageSent = false;
            while (true)
            {
                try
                {
                    return await GetBundleUriImpl();
                }
                catch (BundleNotFoundException e)
                {
                    if (!messageSent)
                    {
                        _logger.Error($"Retry bundle in 3 sec: {e.Message}");
                        messageSent = true;
                    }

                    Thread.Sleep(BundleRetryMsec);
                }
            }
        }


        private async Task<string> GetBundleUriImpl()
        {
            var serverIdentity = GetServerIdentity();
            var response = await _requestSender.SendRequest<GetBundleUriResponse>(_config.RouterUrl,
                new GetBundleUriRequest(serverIdentity));

            if (!response.Success)
            {
                throw new BundleNotFoundException($"Bundle not found for: {serverIdentity}");
            }

            _logger.Error($"Bundle uri received for '{serverIdentity}': {response.BundleUri}");
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