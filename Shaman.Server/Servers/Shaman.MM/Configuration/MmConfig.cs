using System.Collections.Generic;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Messages;
using Shaman.Messages.General.Entity;

namespace Shaman.MM.Configuration
{
    public class MmApplicationConfig : ApplicationConfig
    {
        public int ServerInactivityTimeoutMs { get; set; }
        public int ServerUnregisterTimeoutMs { get; set; }
        
        public int ActualizeMatchmakerIntervalMs { get; set; }
        public GameProject GameProject { get; set; }
        public int ServerInfoListUpdateIntervalMs { get; set; }
        
        public MmApplicationConfig(
            string region,
            string publicDomainNameOrIpAddress, 
            List<ushort> ports, 
            string routerUrl, 
            int serverInactivityTimeoutMs, 
            int serverUnregisterTimeoutMs,
            GameProject gameProject,
            string name,
            int socketTickTimeMs = 100, 
            int receiveTickTimeMs = 33,
            int sendTickTimeMs = 50,
            int getBackendListFromRouterIntervalMs = 30000,
            int actualizeMatchmakerIntervalMs = 15000,
            bool isAuthOn = true,
            string authSecret = null,
            SocketType socketType = SocketType.BareSocket,
            int serverInfoListUpdateIntervalMs = 60000)
            : base(name, region, ServerRole.MatchMaker, publicDomainNameOrIpAddress, ports, routerUrl, socketTickTimeMs, receiveTickTimeMs, sendTickTimeMs,
                socketType, getBackendListFromRouterIntervalMs: getBackendListFromRouterIntervalMs, isAuthOn: isAuthOn,
                authSecret: authSecret)
        {
            ServerInactivityTimeoutMs = serverInactivityTimeoutMs;
            ServerUnregisterTimeoutMs = serverUnregisterTimeoutMs;
            ActualizeMatchmakerIntervalMs = actualizeMatchmakerIntervalMs;
            ServerInfoListUpdateIntervalMs = serverInfoListUpdateIntervalMs;
            GameProject = gameProject;
        }
    }
}