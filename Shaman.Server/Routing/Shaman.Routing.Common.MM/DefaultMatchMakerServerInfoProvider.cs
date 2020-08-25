using Shaman.Common.Server.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Routing.Common.MM
{
    public class DefaultMatchMakerServerInfoProvider : IMatchMakerServerInfoProvider
    {
        public void Start()
        {
        }

        public void Stop()
        {
        }

        public EntityDictionary<ServerInfo> GetGameServers()
        {
            return new EntityDictionary<ServerInfo>();
        }

        public ServerInfo GetServer(int serverId)
        {
            return new ServerInfo();
        }

        public ServerInfo GetLessLoadedServer()
        {
            return new ServerInfo();
        }
    }
}