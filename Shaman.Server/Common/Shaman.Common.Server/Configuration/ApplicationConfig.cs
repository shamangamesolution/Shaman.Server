namespace Shaman.Common.Server.Configuration
{
    public class ApplicationConfig : IApplicationConfig
    {
        public SocketType SocketType;

        public ushort[] Ports;
        public int SocketTickTimeMs;
        public int ReceiveTickTimeMs;
        public int SendTickTimeMs;
        public string PublicDomainNameOrAddress;
        public bool AuthOn;
        public int BackendListFromRouterIntervalMs;
        public string RouterUrl;
        public int MaxPacketSize;
        
        public ApplicationConfig(string publicDomainNameOrIpAddress, ushort[] ports, string routerUrl, int socketTickTimeMs = 100, int receiveTickTimeMs = 33, int sendTickTimeMs = 50, SocketType socketType = SocketType.BareSocket, bool isAuthOn = true, int getBackendListFromRouterIntervalMs = 30000, int maxPacketSize = 300)
        {
            Initialize(publicDomainNameOrIpAddress, ports, socketTickTimeMs, receiveTickTimeMs, sendTickTimeMs, routerUrl, socketType, isAuthOn, getBackendListFromRouterIntervalMs, maxPacketSize);
        }
        
        public void Initialize(string publicDomainNameOrIpAddress, ushort[] ports, int socketTickTimeMs, int receiveTickTimeMs, int sendTickTimeMs, string routerUrl, SocketType socketType = SocketType.BareSocket, bool isAuthOn = true, int getBackendListFromRouterIntervalMs = 30000, int maxPacketSize = 300)
        {
            Ports = ports;
            SocketTickTimeMs = socketTickTimeMs;
            ReceiveTickTimeMs = receiveTickTimeMs;
            PublicDomainNameOrAddress = publicDomainNameOrIpAddress;
            AuthOn = isAuthOn;
            BackendListFromRouterIntervalMs = getBackendListFromRouterIntervalMs;
            RouterUrl = routerUrl;
            SendTickTimeMs = sendTickTimeMs;
            MaxPacketSize = maxPacketSize;
            SocketType = socketType;
        }

        public string GetPublicName()
        {
            return PublicDomainNameOrAddress;
        }

        public ushort[] GetListenPorts()
        {
            return Ports;
        }

        public int GetSocketTickTimeMs()
        {
            return SocketTickTimeMs;
        }

        public int GetReceiveTickTimerMs()
        {
            return ReceiveTickTimeMs;
        }

        public bool IsAuthOn()
        {
            return AuthOn;
        }

        public SocketType GetSocketType()
        {
            return SocketType;
        }

        public int GetBackendListFromRouterIntervalMs()
        {
            return BackendListFromRouterIntervalMs;
        }

        public string GetRouterUrl()
        {
            return RouterUrl;
        }

        public int GetSendTickTimerMs()
        {
            return SendTickTimeMs;
        }

        public int GetMaxPacketSize()
        {
            return MaxPacketSize;
        }
    }
}