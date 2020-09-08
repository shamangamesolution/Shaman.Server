using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.MM.Balancing
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            Bootstrap.LaunchWithCommonAndRoleConfig<Startup>(ServerRole.MatchMaker, (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}