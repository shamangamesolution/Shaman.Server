using System.Collections.Generic;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;

namespace Shaman.MM.Configuration
{
    public class MmApplicationConfig : ApplicationConfig
    {
        public int ServerUnregisterTimeoutMs { get; set; }
        public int ServerInfoListUpdateIntervalMs { get; set; }
        
        // public MmApplicationConfig(
        //     string region,
        //     string publicDomainNameOrIpAddress, 
        //     List<ushort> ports, 
        //     string routerUrl, 
        //     string name,
        //     ushort httpPort,
        //     int socketTickTimeMs, 
        //     int receiveTickTimeMs,
        //     int sendTickTimeMs,
        //     bool isAuthOn,
        //     string authSecret,
        //     SocketType socketType,
        //     int actualizationIntervalMs,
        //     bool overwriteDownloadedBundle,
        //     int maxPacketSize,
        //     int basePacketBufferSize)
        //     : base(name, region, ServerRole.MatchMaker, publicDomainNameOrIpAddress, ports, routerUrl, httpPort, socketTickTimeMs, receiveTickTimeMs, sendTickTimeMs,
        //         socketType, isAuthOn, authSecret, maxPacketSize, basePacketBufferSize, actualizationIntervalMs, overwriteDownloadedBundle)
        // {
        //
        // }

        public void InitializeAdditionalParameters(int serverUnregisterTimeoutMs, int serverInfoListUpdateIntervalMs)
        {
            ServerUnregisterTimeoutMs = serverUnregisterTimeoutMs;
            ServerInfoListUpdateIntervalMs = serverInfoListUpdateIntervalMs;
        }
    }
}