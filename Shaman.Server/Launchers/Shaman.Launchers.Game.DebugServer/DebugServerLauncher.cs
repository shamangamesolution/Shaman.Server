using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Bundle;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Game.DebugServer
{
    /// <summary>
    /// Used for launching standalone configuration - early bound game bundle passed directly to launcher
    /// </summary>
    public static class DebugServerLauncher
    {
        public class LaunchResult
        {
            public Task ServerTask { get; internal set; }
        }

        internal static IGameBundle StandaloneBundle { get; set; }
        private static string _bindToIp;
        
        public static LaunchResult Launch(IGameBundle bundle,
             IApplicationConfig applicationConfig, string bindToIp, string levelLog)
        {
            StandaloneBundle = bundle;
            _bindToIp = bindToIp;
            
            var config = BuildConfig(applicationConfig, levelLog);
            var serverTask = Task.Factory.StartNew(() => Bootstrap.Launch<Startup>(config));

            return new LaunchResult
            {
                ServerTask = serverTask,
            };
        }

        private static IConfigurationRoot BuildConfig(IApplicationConfig config, string levelLog)
        {
            if (!config.ListenPorts.Any())
                throw new Exception($"No UDP port to listen");
            
            return new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new[]
                    {
                        new KeyValuePair<string, string>("CommonSettings:SocketTickTimeMs", config.SocketTickTimeMs.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:ReceiveTickTimeMs", config.ReceiveTickTimeMs.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:SendTickTimeMs", config.SendTickTimeMs.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:MaxPacketSize", config.MaxPacketSize.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:BasePacketBufferSize", config.BasePacketBufferSize.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:ListenPorts", config.ListenPorts),
                        new KeyValuePair<string, string>("CommonSettings:IsAuthOn", config.IsAuthOn.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:ConsoleLogLevel", levelLog),
                        new KeyValuePair<string, string>("Serilog:MinimumLevel", levelLog),
                        new KeyValuePair<string, string>("Serilog:customerToken", ""),
                        new KeyValuePair<string, string>("CommonSettings:BindToIP", _bindToIp),
                        new KeyValuePair<string, string>("CommonSettings:BindToPortHttp", config.BindToPortHttp.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:IsConnectionDdosProtectionOn", config.IsConnectionDdosProtectionOn.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:MaxConnectsFromSingleIp", config.MaxConnectsFromSingleIp.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:ConnectionCountCheckIntervalMs", config.ConnectionCountCheckIntervalMs.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:BanCheckIntervalMs", config.BanCheckIntervalMs.ToString()),
                        new KeyValuePair<string, string>("CommonSettings:BanDurationMs", config.BanDurationMs.ToString()),
                    }
                })
                .AddEnvironmentVariables()
                .Build();
        }
    }
}