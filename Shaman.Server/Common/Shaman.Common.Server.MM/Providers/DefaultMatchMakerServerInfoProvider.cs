using Shaman.Common.Server.Messages;
using Shaman.Messages.General.Entity;
using Shaman.Router.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Common.Server.MM.Providers
{
    public class DefaultMatchMakerServerInfoProvider : IMatchMakerServerInfoProvider
    {
        public void Start()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public EntityDictionary<ServerInfo> GetGameServers()
        {
            throw new System.NotImplementedException();
        }

        public ServerInfo GetServer(int serverId)
        {
            throw new System.NotImplementedException();
        }

        public ServerInfo GetLessLoadedServer()
        {
            throw new System.NotImplementedException();
        }
    }
}