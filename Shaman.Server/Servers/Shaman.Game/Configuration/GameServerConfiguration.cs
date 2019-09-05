using Shaman.Common.Server.Configuration;

namespace Shaman.Game.Configuration
{
    public class GameApplicationConfig : ApplicationConfig
    {
        public int ActualizationTimeoutMs { get; set; }
        public string MatchMakerUrl { get; set; }
        public int DestroyEmptyRoomOnMs { get; set; }
        public ushort BindToPortHttp { get; set; }
        
        public GameApplicationConfig(
            string publicDomainNameOrIpAddress, 
            ushort[] ports, 
            string routerUrl, 
            string matchMakerUrl, 
            ushort httpPort, 
            int destroyEmptyRoomOnMs = 60000, 
            int actualizationTimeoutMs = 1000, 
            int getBackendListFromRouterIntervalMs = 30000, 
            bool isAuthOn = true, 
            int socketTickTimeMs = 100, 
            int receiveTickTimeMs = 33, 
            int sendTickTimeMs = 50,
            SocketType socketType = SocketType.BareSocket) 
            : base(publicDomainNameOrIpAddress, ports, routerUrl, socketType: socketType, getBackendListFromRouterIntervalMs: getBackendListFromRouterIntervalMs, isAuthOn:isAuthOn, socketTickTimeMs:socketTickTimeMs, receiveTickTimeMs:receiveTickTimeMs, sendTickTimeMs:sendTickTimeMs)
        {
            MatchMakerUrl = matchMakerUrl;
            DestroyEmptyRoomOnMs = destroyEmptyRoomOnMs;
            ActualizationTimeoutMs = actualizationTimeoutMs;
            BindToPortHttp = httpPort;
        }
    }
}