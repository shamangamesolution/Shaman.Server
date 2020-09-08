using Shaman.Contract.Routing;

namespace Shaman.Launchers.Game.Balancing
{
    public interface IServerIdentityProvider
    {
        ServerIdentity Get();
    }

    public class DefaultServerIdentityProvider : IServerIdentityProvider
    {
        private readonly ServerIdentity _serverIdentity;

        public DefaultServerIdentityProvider(ServerIdentity serverIdentity)
        {
            _serverIdentity = serverIdentity;
        }

        public ServerIdentity Get()
        {
            return _serverIdentity;
        }
    }
}