using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Game.Balancing
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            Bootstrap.Launch<Startup>(SourceType.GameServer, (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}