using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Launchers.Common;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Game
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