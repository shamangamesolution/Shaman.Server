using System.Collections.Generic;
using Shaman.Contract.Routing;

namespace Shaman.Common.Server.Configuration
{
    public class ApplicationConfig : IApplicationConfig
    {
        public string PublicDomainNameOrAddress { get; set; }
        public string ListenPorts { get; set; }
        public int SocketTickTimeMs { get; set; }
        public int ReceiveTickTimeMs { get; set; }
        public int SendTickTimeMs { get; set; }
        public bool IsAuthOn { get; set; }
        public SocketType SocketType { get; set; }
        public string AuthSecret { get; set; }
        public string ServerName { get; set; }
        public string Region { get; set; }
        public ServerRole ServerRole { get; set; }

        public int MaxPacketSize { get; set; }
        public int BasePacketBufferSize { get; set; }
        private ServerIdentity _identity;
        public ushort BindToPortHttp { get; set; }
        public int MaxConnectsFromSingleIp { get; set; }
        public int ConnectionCountCheckIntervalMs { get; set; }
        public int BanCheckIntervalMs { get; set; }
        public int BanDurationMs { get; set; }
        public bool IsConnectionDdosProtectionOn { get; set; }

        public ServerIdentity GetIdentity()
        {
            return new ServerIdentity(PublicDomainNameOrAddress, ListenPorts, ServerRole);;
        }
    }
}