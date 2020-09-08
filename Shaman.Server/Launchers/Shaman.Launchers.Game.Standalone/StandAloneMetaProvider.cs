using Shaman.Contract.Bundle;
using Shaman.Contract.Routing.Meta;

namespace Shaman.Launchers.Game.Standalone
{
    class StandAloneMetaProvider : IMetaProvider
    {
        private string _url;
        private readonly IBundleConfig _bundleConfig;
        
        public StandAloneMetaProvider(IBundleConfig bundleConfig)
        {
            _bundleConfig = bundleConfig;
            _url = bundleConfig.GetValueOrNull("MetaUrl");
        }
        
        public string GetFirstMetaServerUrl()
        {
            return _url;
        }

        public string GetMetaServerUrl(int id)
        {
            return _url;
        }

        public void Start(int getBackendListIntervalMs = 1000)
        {
            
        }

        public void Stop()
        {
        }

        public void Start()
        {
        }
    }
}