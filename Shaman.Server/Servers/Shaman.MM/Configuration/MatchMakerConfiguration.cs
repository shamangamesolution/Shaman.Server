using System.Collections.Generic;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;

namespace Shaman.MM.Configuration
{
    public class MmApplicationConfig : ApplicationConfig
    {
        public int ServerUnregisterTimeoutMs { get; set; }
        public int ServerInfoListUpdateIntervalMs { get; set; }
        
        public MmApplicationConfig(
            string region,
            string publicDomainNameOrIpAddress, 
            List<ushort> ports, 
            string routerUrl, 
            int serverUnregisterTimeoutMs,
            string name,
            ushort httpPort,
            int socketTickTimeMs = 100, 
            int receiveTickTimeMs = 33,
            int sendTickTimeMs = 50,
            bool isAuthOn = true,
            string authSecret = null,
            SocketType socketType = SocketType.BareSocket,
            int serverInfoListUpdateIntervalMs = 60000,
            int actualizationIntervalMs = 1000)
            : base(name, region, ServerRole.MatchMaker, publicDomainNameOrIpAddress, ports, routerUrl, httpPort, socketTickTimeMs, receiveTickTimeMs, sendTickTimeMs,
                socketType, isAuthOn: isAuthOn,
                authSecret: authSecret, actualizationIntervalMs:actualizationIntervalMs)
        {
            ServerUnregisterTimeoutMs = serverUnregisterTimeoutMs;
            ServerInfoListUpdateIntervalMs = serverInfoListUpdateIntervalMs;
        }
    }
}