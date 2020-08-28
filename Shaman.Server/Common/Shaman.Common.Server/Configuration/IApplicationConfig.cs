using System.Collections.Generic;
using Shaman.Common.Udp.Senders;
using Shaman.Contract.Routing;

namespace Shaman.Common.Server.Configuration
{
    public enum SocketType : byte
    {
        BareSocket = 1,
        ThreadSocket = 2
    }
    
    public interface IApplicationConfig: IPacketSenderConfig
    {
        string PublicDomainNameOrAddress { get; set; }
        List<ushort> ListenPorts { get; set; }
        int SocketTickTimeMs { get; set; }
        int ReceiveTickTimeMs { get; set; }
        bool IsAuthOn { get; set; }
        SocketType SocketType { get; set; }
        string AuthSecret { get; set; }
        string ServerName { get; set; }
        string Region { get; set; }
        ServerRole ServerRole { get; set; }
        ushort BindToPortHttp { get; set; }
        ServerIdentity GetIdentity();

    }
}