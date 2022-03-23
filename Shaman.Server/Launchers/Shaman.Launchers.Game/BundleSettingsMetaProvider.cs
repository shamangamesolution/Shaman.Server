using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.Meta;

namespace Shaman.Launchers.Game
{
    public class BundleSettingsMetaProvider : IMetaProvider
    {
        private string _metaUrl;
        private readonly IShamanLogger _logger;
        
        public BundleSettingsMetaProvider(IBundleConfig bundleConfig, IShamanLogger logger)
        {
            _logger = logger;
            _metaUrl = bundleConfig.GetValueOrNull("MetaUrl");
            if (string.IsNullOrWhiteSpace(_metaUrl))
                _logger.Error($"Meta server url was not set in bundle settings file");
        }

        public string GetFirstMetaServerUrl()
        {
            return _metaUrl;
        }

        public string GetMetaServerUrl(int id)
        {
            return _metaUrl;
        }

        public void Start(int getBackendListIntervalMs = 1000)
        {
        }

        public void Stop()
        {
        }
    }
}