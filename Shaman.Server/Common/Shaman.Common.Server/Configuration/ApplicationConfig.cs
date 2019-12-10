using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Servers;

namespace Shaman.Common.Server.Configuration
{
    public class ApplicationConfig : IApplicationConfig
    {
        private readonly SocketType _socketType;

        private readonly List<ushort> _ports;
        private readonly int _socketTickTimeMs;
        private readonly int _receiveTickTimeMs;
        private readonly int _sendTickTimeMs;
        private readonly string _publicDomainNameOrAddress;
        private readonly bool _authOn;
        private readonly int _backendListFromRouterIntervalMs;
        private readonly string _routerUrl;
        private readonly int _maxPacketSize;
        private readonly string _authSecret;
        private readonly string _name;
        private readonly string _region;
        private readonly ServerRole _serverRole;
        private ServerIdentity _identity;
        
        public ApplicationConfig(string name, string region, ServerRole serverRole, string publicDomainNameOrIpAddress, List<ushort> ports, string routerUrl, int socketTickTimeMs = 10, int receiveTickTimeMs = 33, int sendTickTimeMs = 50, SocketType socketType = SocketType.BareSocket, bool isAuthOn = true, string authSecret = null, int getBackendListFromRouterIntervalMs = 30000, int maxPacketSize = 300)
        {
            _name = name;
            _region = region;
            _serverRole = serverRole;
            _ports = ports;
            _socketTickTimeMs = socketTickTimeMs;
            _receiveTickTimeMs = receiveTickTimeMs;
            _publicDomainNameOrAddress = publicDomainNameOrIpAddress;
            _authOn = isAuthOn;
            _authSecret = authSecret;
            _backendListFromRouterIntervalMs = getBackendListFromRouterIntervalMs;
            _routerUrl = routerUrl;
            _sendTickTimeMs = sendTickTimeMs;
            _maxPacketSize = maxPacketSize;
            _socketType = socketType;
            _identity = new ServerIdentity(publicDomainNameOrIpAddress, ports, serverRole);
        }

        public string GetPublicName()
        {
            return _publicDomainNameOrAddress;
        }

        public List<ushort> GetListenPorts()
        {
            return _ports;
        }

        public int GetSocketTickTimeMs()
        {
            return _socketTickTimeMs;
        }

        public int GetReceiveTickTimerMs()
        {
            return _receiveTickTimeMs;
        }

        public bool IsAuthOn()
        {
            return _authOn;
        }

        public SocketType GetSocketType()
        {
            return _socketType;
        }

        public int GetBackendListFromRouterIntervalMs()
        {
            return _backendListFromRouterIntervalMs;
        }

        public string GetRouterUrl()
        {
            return _routerUrl;
        }

        public int GetSendTickTimerMs()
        {
            return _sendTickTimeMs;
        }

        public int GetMaxPacketSize()
        {
            return _maxPacketSize;
        }

        public string GetAuthSecret()
        {
            return _authSecret;
        }

        public string GetServerName()
        {
            return _name;
        }

        public string GetRegion()
        {
            return _region;
        }

        public ServerRole GetServerRole()
        {
            return _serverRole;
        }

        public ServerIdentity GetIdentity()
        {
            return _identity;
        }
    }
}