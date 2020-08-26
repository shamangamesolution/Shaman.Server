using System;
using Microsoft.Extensions.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.MM.Balancing
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            Bootstrap.LaunchWithCommonAndRoleConfig<Startup>(ServerRole.GameServer, (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}