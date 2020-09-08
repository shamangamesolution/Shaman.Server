using System;
using Shaman.Contract.Bundle;
using Shaman.Contract.Routing.Meta;

namespace Shaman.Launchers.Game
{
    public class BundleSettingsMetaProvider : IMetaProvider
    {
        private string _metaUrl;

        public BundleSettingsMetaProvider(IBundleConfig bundleConfig)
        {
            _metaUrl = bundleConfig.GetValueOrNull("MetaUrl");
            if (string.IsNullOrWhiteSpace(_metaUrl))
                throw new Exception($"Meta url is null");
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