using System;
using Microsoft.Extensions.Configuration;
using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Game.Balancing
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            Bootstrap.LaunchWithCommonAndRoleConfig<Startup>(ServerRole.GameServer.ToString(), (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}