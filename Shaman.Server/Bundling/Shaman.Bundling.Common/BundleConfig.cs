using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using Shaman.Contract.Bundle;

namespace Shaman.Bundling.Common
{
    public class BundleConfig : IBundleConfig
    {
        private readonly IBundleSettingsProvider _settingsProvider;
        
        private Dictionary<string, string> _parameters = null;

        public BundleConfig(IBundleSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }
        
        public string GetValueOrNull(string key)
        {
            if (_parameters == null)
                _parameters = _settingsProvider.GetSettings();
            
            if (!_parameters.TryGetValue(key, out var result))
                return null;

            return result;
        }
    }
}