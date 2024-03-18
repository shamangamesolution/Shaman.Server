using System.Threading.Tasks;
using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Game
{
    public static class Program
    {
        internal static async Task Main(string[] args)
        {
            //launch game server
            await Bootstrap.LaunchWithCommonAndRoleConfig<Startup>(ServerRole.GameServer.ToString(), (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}