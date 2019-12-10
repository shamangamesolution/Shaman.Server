using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Servers;

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
        string GetRouterUrl();
        string GetAuthSecret();
        string GetServerName();
        string GetRegion();
        ServerRole GetServerRole();
        ServerIdentity GetIdentity();
    }
}