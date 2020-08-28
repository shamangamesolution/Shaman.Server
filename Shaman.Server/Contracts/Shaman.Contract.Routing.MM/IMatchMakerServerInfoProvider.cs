using Shaman.Serialization.Messages;

namespace Shaman.Contract.Routing.MM
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