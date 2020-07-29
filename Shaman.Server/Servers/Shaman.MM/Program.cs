using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Utils.Logging;
using Shaman.ServerSharedUtilities;

namespace Shaman.MM
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