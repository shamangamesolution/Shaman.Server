using Shaman.Common.Server.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Routing.Common.MM
{
    public interface IMatchMakerServerInfoProvider
    {
        void Start();
        void Stop();
        EntityDictionary<ServerInfo> GetGameServers();
        ServerInfo GetServer(int serverId);
        ServerInfo GetLessLoadedServer();
        void AddServer(ServerInfo serverInfo);
    }
}