using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Bundle;
using Shaman.Contract.Routing;
using Shaman.Game.Api;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Game.Standalone
{
    /// <summary>
    /// Used for launching standalone configuration - early bound game bundle passed directly to launcher
    /// </summary>
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
        internal static IApplicationConfig Config { get; set; }
        private static string _levelLog;
        private static string _bindToIp;
        
        public static LaunchResult Launch(IGameBundle bundle,
            string[] args, IApplicationConfig applicationConfig, string bindToIp, string levelLog)
        {
            StandaloneBundle = bundle;
            Config = applicationConfig;
            _levelLog = levelLog;
            _bindToIp = bindToIp;
            
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
            if (!Config.ListenPorts.Any())
                throw new Exception($"No UDP port to listen");
            
            return new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new[]
                    {
                        new KeyValuePair<string, string>("CommonSettings:SocketTickTimeMs", Config.SocketTickTimeMs.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:ReceiveTickTimeMs", Config.ReceiveTickTimeMs.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:SendTickTimeMs", Config.SendTickTimeMs.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:MaxPacketSize", Config.MaxPacketSize.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:BasePacketBufferSize", Config.BasePacketBufferSize.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:ListenPorts", Config.ListenPorts.First().ToString()),
                        new KeyValuePair<string, string>("CommonSettings:IsAuthOn", Config.IsAuthOn.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:ConsoleLogLevel", _levelLog),
                        new KeyValuePair<string, string>("Serilog:MinimumLevel", _levelLog),
                        new KeyValuePair<string, string>("Serilog:customerToken", ""),
                        new KeyValuePair<string, string>("CommonSettings:BindToIP", _bindToIp),
                        new KeyValuePair<string, string>("CommonSettings:BindToPortHttp", Config.BindToPortHttp.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:SocketType", Config.SocketType.ToString()),
                    }
                })
                .Build();
        }
    }
}