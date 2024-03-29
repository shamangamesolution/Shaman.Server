using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Shaman.Bundling.Common;

namespace Shaman.Launchers.Common
{
    /// <summary>
    /// Loads bundle settings from separated bundle
    /// </summary>
    public class BundleSettingsFromBundleLoaderProvider : IBundleSettingsProvider
    {
        private readonly IBundleLoader _bundleLoader;

        public BundleSettingsFromBundleLoaderProvider(IBundleLoader bundleLoader)
        {
            _bundleLoader = bundleLoader;
        }
        
        public Dictionary<string, string> GetSettings()
        {
            var dictionary = new Dictionary<string, string>();
            //TODO file order - we don't know whick config comes first
            foreach (var config in _bundleLoader.GetConfigs().Where(c=> c.EndsWith("appsettings.bundle.json") || c.EndsWith($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.bundle.json")))
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