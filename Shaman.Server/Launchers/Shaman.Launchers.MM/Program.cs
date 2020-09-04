using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.MM
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            //start MM
            Bootstrap.LaunchWithCommonAndRoleConfig<Startup>(ServerRole.MatchMaker, (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}