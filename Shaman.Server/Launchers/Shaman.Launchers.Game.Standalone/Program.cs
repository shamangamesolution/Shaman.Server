using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Game.Standalone
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            //launch game server
            Bootstrap.LaunchWithCommonAndRoleConfig<Startup>(ServerRole.GameServer.ToString(), (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}