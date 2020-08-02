using System.Collections.Generic;
using Shaman.Common.Server.Configuration;
using Shaman.Messages.General.Entity;
using Shaman.Router.Messages;

namespace Shaman.MM.Configuration
{
    public class MmApplicationConfig : ApplicationConfig
    {
        public int ServerUnregisterTimeoutMs { get; set; }
        public int ActualizeMatchmakerIntervalMs { get; set; }
        public GameProject GameProject { get; set; }
        public int ServerInfoListUpdateIntervalMs { get; set; }
        
        public MmApplicationConfig(
            string region,
            string publicDomainNameOrIpAddress, 
            List<ushort> ports, 
            string routerUrl, 
            int serverUnregisterTimeoutMs,
            GameProject gameProject,
            string name,
            ushort httpPort,
            int socketTickTimeMs = 100, 
            int receiveTickTimeMs = 33,
            int sendTickTimeMs = 50,
            int getBackendListFromRouterIntervalMs = 30000,
            int actualizeMatchmakerIntervalMs = 15000,
            bool isAuthOn = true,
            string authSecret = null,
            SocketType socketType = SocketType.BareSocket,
            int serverInfoListUpdateIntervalMs = 60000)
            : base(name, region, ServerRole.MatchMaker, publicDomainNameOrIpAddress, ports, routerUrl, httpPort, socketTickTimeMs, receiveTickTimeMs, sendTickTimeMs,
                socketType, getBackendListFromRouterIntervalMs: getBackendListFromRouterIntervalMs, isAuthOn: isAuthOn,
                authSecret: authSecret)
        {
            ServerUnregisterTimeoutMs = serverUnregisterTimeoutMs;
            ActualizeMatchmakerIntervalMs = actualizeMatchmakerIntervalMs;
            ServerInfoListUpdateIntervalMs = serverInfoListUpdateIntervalMs;
            GameProject = gameProject;
        }
    }
}