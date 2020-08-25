using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.MM
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.common.json", optional: false)
                .AddJsonFile("appsettings.common.mm.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            Bootstrap.Launch<Startup>(ServerRole.MatchMaker, config, (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}