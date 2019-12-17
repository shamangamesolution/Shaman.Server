using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Shaman.Game.Configuration;
using Shaman.Game.Contract;

namespace Shaman.Game
{
    public static class StandaloneServerLauncher
    {
        internal static bool IsStandaloneMode => StandaloneBundle != null;
        internal static IGameBundle StandaloneBundle { get; set; }
        internal static GameApplicationConfig Config { get; set; }

        public static void Launch(IGameBundle bundle,
            string[] args,
            string name,
            string regionName,
            string publicDomainNameOrIpAddress,
            List<ushort> ports,
            ushort httpPort,
            int destroyEmptyRoomOnMs = 60000,
            string authSecret = null,
            int socketTickTimeMs = 100,
            int receiveTickTimeMs = 33,
            int sendTickTimeMs = 50,
            int serverInfoListUpdateIntervalMs = 60000)
        {
            StandaloneBundle = bundle;
            Config = new GameApplicationConfig(name, regionName, publicDomainNameOrIpAddress, ports, String.Empty,
                String.Empty, httpPort, isAuthOn: false);
            var config = BuildConfig();
            Program.Start(config);
        }

        private static IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new[]
                    {
                        new KeyValuePair<string, string>("Serilog:MinimumLevel", "Error"),
                        new KeyValuePair<string, string>("ConsoleLogLevel", "Error"),
                        new KeyValuePair<string, string>("Serilog:customerToken", null),
                        new KeyValuePair<string, string>("BindToIP", "0.0.0.0"),
                        new KeyValuePair<string, string>("BindToPortHttp", Config.BindToPortHttp.ToString()),
                    }
                })
                .Build();
        }
    }
}