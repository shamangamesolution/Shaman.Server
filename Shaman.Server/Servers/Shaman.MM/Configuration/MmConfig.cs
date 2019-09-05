using Shaman.Common.Server.Configuration;
using Shaman.Messages.General.Entity;

namespace Shaman.MM.Configuration
{
    public class MmApplicationConfig : ApplicationConfig
    {
        public int ServerInactivityTimeoutMs { get; set; }
        public int ServerUnregisterTimeoutMs { get; set; }
        
        public int ActualizeMatchmakerIntervalMs { get; set; }
        public GameProject GameProject { get; set; }
        public string Name { get; set; }
        public string Secret { get; set; }
        
        public MmApplicationConfig(
            string publicDomainNameOrIpAddress, 
            ushort[] ports, 
            string routerUrl, 
            int serverInactivityTimeoutMs, 
            int serverUnregisterTimeoutMs,
            GameProject gameProject,
            string name,
            string secret,
            int socketTickTimeMs = 100, 
            int receiveTickTimeMs = 33,
            int sendTickTimeMs = 50,
            int getBackendListFromRouterIntervalMs = 30000,
            int actualizeMatchmakerIntervalMs = 60000,
            bool isAuthOn = true,
            SocketType socketType = SocketType.BareSocket) 
            : base(publicDomainNameOrIpAddress, ports, routerUrl, socketTickTimeMs, receiveTickTimeMs, sendTickTimeMs, socketType, getBackendListFromRouterIntervalMs:getBackendListFromRouterIntervalMs, isAuthOn:isAuthOn)
        {
            ServerInactivityTimeoutMs = serverInactivityTimeoutMs;
            ServerUnregisterTimeoutMs = serverUnregisterTimeoutMs;
            ActualizeMatchmakerIntervalMs = actualizeMatchmakerIntervalMs;
            GameProject = gameProject;
            Name = name;
            Secret = secret;
        }
    }
}