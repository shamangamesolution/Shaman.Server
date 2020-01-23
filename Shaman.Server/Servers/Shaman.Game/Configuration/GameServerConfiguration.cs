using System.Collections.Generic;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Messages;

namespace Shaman.Game.Configuration
{
    public class GameApplicationConfig : ApplicationConfig
    {
        public int ActualizationTimeoutMs { get; set; }
        public string MatchMakerUrl { get; set; }
        public int DestroyEmptyRoomOnMs { get; set; }
        public int ServerInfoListUpdateIntervalMs { get; set; }
        public bool OverwriteDownloadedBundle { get; set; }

        public GameApplicationConfig(string name,
            string regionName,
            string publicDomainNameOrIpAddress,
            List<ushort> ports,
            string routerUrl,
            string matchMakerUrl,
            ushort httpPort,
            int destroyEmptyRoomOnMs = 60000,
            int actualizationTimeoutMs = 10000,
            int getBackendListFromRouterIntervalMs = 30000,
            bool isAuthOn = true,
            string authSecret = null,
            int socketTickTimeMs = 100,
            int receiveTickTimeMs = 33,
            int sendTickTimeMs = 50,
            SocketType socketType = SocketType.BareSocket,
            int serverInfoListUpdateIntervalMs = 60000)
            : base(name, regionName, ServerRole.GameServer, publicDomainNameOrIpAddress, ports, routerUrl, httpPort, socketType: socketType,
                getBackendListFromRouterIntervalMs: getBackendListFromRouterIntervalMs, isAuthOn: isAuthOn,
                authSecret: authSecret, socketTickTimeMs: socketTickTimeMs, receiveTickTimeMs: receiveTickTimeMs,
                sendTickTimeMs: sendTickTimeMs)
        {
            MatchMakerUrl = matchMakerUrl;
            DestroyEmptyRoomOnMs = destroyEmptyRoomOnMs;
            ActualizationTimeoutMs = actualizationTimeoutMs;
            ServerInfoListUpdateIntervalMs = serverInfoListUpdateIntervalMs;
        }
    }
}