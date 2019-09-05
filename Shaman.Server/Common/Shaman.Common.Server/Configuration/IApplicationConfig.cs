namespace Shaman.Common.Server.Configuration
{
    public enum SocketType : byte
    {
        BareSocket = 1,
        ThreadSocket = 2
    }
    
    public interface IApplicationConfig
    {
        void Initialize(string publicDomainNameOrIpAddress, ushort[] ports, int socketTickTimeMs, int receiveTickTimeMs, int sendTickTimeMs, string routerUrl, SocketType socketType = SocketType.BareSocket, bool isAuthOn = true, int backEndsListRequestIntervalMs = 30000, int maxPacketSize = 300);
        string GetPublicName();
        ushort[] GetListenPorts();
        int GetSocketTickTimeMs();
        int GetReceiveTickTimerMs();
        bool IsAuthOn();
        SocketType GetSocketType();
        int GetBackendListFromRouterIntervalMs();
        string GetRouterUrl();
        int GetSendTickTimerMs();
        int GetMaxPacketSize();
    }
}