using System;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Messages;

namespace Shaman.ServerSharedUtilities.Bundling
{
    public class BundleInfoProvider : IBundleInfoProvider
    {
        private const int BundleRetryMsec = 1500;
        private readonly IRequestSender _requestSender;
        private readonly IServerActualizer _serverActualizer;
        private readonly IShamanLogger _logger;
        private readonly IApplicationConfig _config;

        public BundleInfoProvider(IRequestSender requestSender, IServerActualizer serverActualizer, IApplicationConfig config, IShamanLogger logger)
        {
            _requestSender = requestSender;
            _serverActualizer = serverActualizer;
            _logger = logger;
            _config = config;
        }

        private ServerIdentity GetServerIdentity()
        {
            return new ServerIdentity(_config.GetPublicName(),
                _config.GetListenPorts(), _config.GetServerRole());
        }

        public async Task<string> GetBundleUri()
        {
            var messageSent = false;
            while (true)
            {
                try
                {
                    await _serverActualizer.Actualize(0);
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
            var response = await _requestSender.SendRequest<GetBundleUriResponse>(_config.GetRouterUrl(),
                new GetBundleUriRequest(serverIdentity));

            if (!response.Success)
            {
                throw new BundleNotFoundException($"Bundle not found for: {serverIdentity}");
            }

            _logger.Error($"Bandle uri received for '{serverIdentity}': {response.BundleUri}");
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