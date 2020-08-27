using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Shaman.Bundling.Common
{
    public interface IBundleSettingsProvider
    {
        Dictionary<string, string> GetSettings();
    }
    
    public class BundleSettingsFromFileProvider : IBundleSettingsProvider
    {
        private readonly IBundleLoader _bundleLoader;

        public BundleSettingsFromFileProvider(IBundleLoader bundleLoader)
        {
            _bundleLoader = bundleLoader;
        }
        
        public Dictionary<string, string> GetSettings()
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var config in _bundleLoader.GetConfigs().Where(c => c.EndsWith("appsettings.bundle.json")))
            {
                var text = File.ReadAllText(config);
                var deserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                foreach (var param in deserialized)
                {
                    if (!dictionary.ContainsKey(param.Key))
                        dictionary.Add(param.Key, param.Value);
                }
            }
            return dictionary;
        }
    }
}