using Shaman.Contract.Routing;

namespace Shaman.Launchers.Common.Balancing
{
    public interface IRoutingConfig
    {
        string RouterUrl { get; }
        ServerIdentity Identity { get; }
        string ServerName { get; }
        string Region { get; }
        ushort HttpPort { get; }
        ushort HttpsPort { get; }
    }
    public class RoutingConfig : IRoutingConfig
    {
        public RoutingConfig(string routerUrl, ServerIdentity identity, string serverName, string region, ushort httpPort, ushort httpsPort)
        {
            RouterUrl = routerUrl;
            Identity = identity;
            ServerName = serverName;
            Region = region;
            HttpPort = httpPort;
            HttpsPort = httpsPort;
        }

        public string RouterUrl { get; }
        public ServerIdentity Identity { get; }
        public string ServerName { get; }
        public string Region { get; }
        public ushort HttpPort { get; }
        public ushort HttpsPort { get; }
    }
}