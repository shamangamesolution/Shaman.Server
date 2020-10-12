using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Shaman.Bundling.Common;

namespace Shaman.Launchers.Game.DebugServer
{
    /// <summary>
    /// Loads bundle settings from exe directory
    /// </summary>
    public class BundleSettingsFromFileProvider : IBundleSettingsProvider
    {
        
        public BundleSettingsFromFileProvider()
        {
        }
        
        public Dictionary<string, string> GetSettings()
        {
            var dictionary = new Dictionary<string, string>();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.bundle.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.bundle.json", optional: true)
                .Build();

            foreach (var item in configuration.GetChildren())
            {
                if (!dictionary.ContainsKey(item.Key))
                    dictionary.Add(item.Key, item.Value);
            }

            return dictionary;
        }
    }
}