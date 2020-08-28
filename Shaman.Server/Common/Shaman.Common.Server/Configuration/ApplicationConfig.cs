using System.Collections.Generic;
using Shaman.Contract.Routing;

namespace Shaman.Common.Server.Configuration
{
    public class ApplicationConfig : IApplicationConfig
    {
        public string PublicDomainNameOrAddress { get; set; }
        public List<ushort> ListenPorts { get; set; }
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

        public ServerIdentity GetIdentity()
        {
            return new ServerIdentity(PublicDomainNameOrAddress, ListenPorts, ServerRole);;
        }
    }
}