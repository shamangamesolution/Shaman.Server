using System.Threading.Tasks;
using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.MM.Balancing
{
    public static class Program
    {
        internal static async Task Main(string[] args)
        {
            await Bootstrap.LaunchWithCommonAndRoleConfig<Startup>(ServerRole.MatchMaker.ToString(), (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}