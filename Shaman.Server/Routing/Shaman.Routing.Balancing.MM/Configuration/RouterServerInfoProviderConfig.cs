using Shaman.Common.Server.Messages;

namespace Shaman.Routing.Balancing.MM.Configuration
{
    public interface IRouterServerInfoProviderConfig
    {
        int ServerInfoListUpdateIntervalMs { get; set; }
        int ActualizationIntervalMs { get; set; }
        int ServerUnregisterTimeoutMs { get; set; }
        ServerIdentity Identity { get; set; }
    }

    public class RouterServerInfoProviderConfig : IRouterServerInfoProviderConfig
    {
        public int ServerInfoListUpdateIntervalMs { get; set; }
        public int ActualizationIntervalMs { get; set; }
        public int ServerUnregisterTimeoutMs { get; set; }
        public ServerIdentity Identity { get; set; }

        public RouterServerInfoProviderConfig(int serverInfoListUpdateIntervalMs, int actualizationIntervalMs,
            int serverUnregisterTimeoutMs, ServerIdentity identity)
        {
            ServerInfoListUpdateIntervalMs = serverInfoListUpdateIntervalMs;
            ActualizationIntervalMs = actualizationIntervalMs;
            ServerUnregisterTimeoutMs = serverUnregisterTimeoutMs;
            Identity = identity;
        }
    }
}