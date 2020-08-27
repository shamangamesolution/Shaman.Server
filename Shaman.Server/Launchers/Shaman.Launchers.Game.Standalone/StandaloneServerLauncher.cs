using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Contract.Bundle;
using Shaman.Game.Api;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Game.Standalone
{
    
    public static class StandaloneServerLauncher
    {
        public class LaunchResult
        {
            public Task ServerTask { get; internal set; }
            public Task<IGameServerApi> ApiInitializationTask { get; internal set; }
        }

        internal static bool IsStandaloneMode => StandaloneBundle != null;
        internal static IGameBundle StandaloneBundle { get; set; }
        internal static IGameServerApi Api { get; set; }
        internal static ApplicationConfig Config { get; set; }

        public static LaunchResult Launch(IGameBundle bundle,
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
            Config = new ApplicationConfig
            {
                ServerName = name,
                Region = regionName,
                PublicDomainNameOrAddress = publicDomainNameOrIpAddress,
                ListenPorts = ports,
                BindToPortHttp = httpPort,
                IsAuthOn = false
            };

            var config = BuildConfig();
            var serverTask = Task.Factory.StartNew(() => Bootstrap.Launch<Launchers.Game.Standalone.Startup>(ServerRole.GameServer, config));

            return new LaunchResult
            {
                ServerTask = serverTask,
                ApiInitializationTask = Task<IGameServerApi>.Factory.StartNew(() =>
                {
                    while (serverTask.Status == TaskStatus.Running && Api == null)
                    {
                        Thread.Sleep(10);
                    }

                    if (Api == null)
                    {
                        throw new Exception("API not initialized");
                    }

                    return Api;
                })
            };
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