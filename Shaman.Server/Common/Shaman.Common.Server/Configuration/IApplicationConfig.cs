using System.Collections.Generic;
using Shaman.Common.Server.Messages;
using Shaman.Common.Udp.Senders;
using Shaman.Router.Messages;

namespace Shaman.Common.Server.Configuration
{
    public enum SocketType : byte
    {
        BareSocket = 1,
        ThreadSocket = 2
    }
    
    public interface IApplicationConfig: IPacketSenderConfig
    {
//        void Initialize(string publicDomainNameOrIpAddress, ushort[] ports, int socketTickTimeMs, int receiveTickTimeMs, int sendTickTimeMs, string routerUrl, SocketType socketType = SocketType.BareSocket, bool isAuthOn = true, int backEndsListRequestIntervalMs = 30000, int maxPacketSize = 300);
        string GetPublicName();
        List<ushort> GetListenPorts();
        int GetSocketTickTimeMs();
        int GetReceiveTickTimerMs();
        bool IsAuthOn();
        SocketType GetSocketType();
        int GetBackendListFromRouterIntervalMs();
        string GetAuthSecret();
        string GetServerName();
        string GetRegion();
        ServerRole GetServerRole();
        ushort BindToPortHttp { get; set; }
        ServerIdentity GetIdentity();
        string GetRouterUrl();
    }
}