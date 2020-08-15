using System.Collections.Generic;
using Shaman.Router.Messages;

namespace Shaman.Router.Backend
{
    public interface IServerIdentityProvider
    {
        ServerIdentity Get();
    }

    public class ServerIdentityProvider : IServerIdentityProvider
    {
        private readonly ServerIdentity _serverIdentity;

        public ServerIdentityProvider(string hostAddress, IEnumerable<ushort> ports, ServerRole serverRole)
        {
            _serverIdentity = new ServerIdentity(hostAddress, ports, serverRole);
        }

        public ServerIdentity Get()
        {
            return _serverIdentity;
        }
    }
}