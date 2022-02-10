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
        private const string CommonAppSettingsFileName = "appsettings.bundle.json";
        private readonly IBundleLoader _bundleLoader;

        private static readonly string EnvAppSettingsFileName =
            $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.bundle.json";

        public BundleSettingsFromBundleLoaderProvider(IBundleLoader bundleLoader)
        {
            _bundleLoader = bundleLoader;
        }

        public Dictionary<string, string> GetSettings()
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var config in _bundleLoader.GetConfigs().Where(c =>
                             c.EndsWith(CommonAppSettingsFileName) ||
                             c.EndsWith(EnvAppSettingsFileName))
                         .OrderBy(r => r.Length))
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