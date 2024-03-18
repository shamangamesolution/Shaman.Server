using System.Threading.Tasks;
using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.MM
{
    public static class Program
    {
        internal static async Task Main(string[] args)
        {
            //start MM
            await Bootstrap.LaunchWithCommonAndRoleConfig<Startup>(ServerRole.MatchMaker.ToString(), (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}