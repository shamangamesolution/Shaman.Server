using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.MM
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            Bootstrap.Launch<Startup>(SourceType.MatchMaker, (loggerConfiguration, appConfig) =>
                loggerConfiguration.Enrich.WithProperty("node",
                    $"{appConfig["PublicDomainNameOrAddress"]}:{appConfig["BindToPortHttp"]}[{appConfig["Ports"]}]"));
        }
    }
}