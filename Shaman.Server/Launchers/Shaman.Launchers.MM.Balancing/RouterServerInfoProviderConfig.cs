using Shaman.Contract.Routing;

namespace Shaman.Launchers.MM.Balancing
{
    public interface IRouterServerInfoProviderConfig
    {
        int ServerInfoListUpdateIntervalMs { get; set; }
        int ServerUnregisterTimeoutMs { get; set; }
        ServerIdentity Identity { get; set; }
    }

    public class RouterServerInfoProviderConfig : IRouterServerInfoProviderConfig
    {
        public int ServerInfoListUpdateIntervalMs { get; set; }
        public int ServerUnregisterTimeoutMs { get; set; }
        public ServerIdentity Identity { get; set; }

        public RouterServerInfoProviderConfig(int serverInfoListUpdateIntervalMs,
            int serverUnregisterTimeoutMs, ServerIdentity identity)
        {
            ServerInfoListUpdateIntervalMs = serverInfoListUpdateIntervalMs;
            ServerUnregisterTimeoutMs = serverUnregisterTimeoutMs;
            Identity = identity;
        }
    }
}