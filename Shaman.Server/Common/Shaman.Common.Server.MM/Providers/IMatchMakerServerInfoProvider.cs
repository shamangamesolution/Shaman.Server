using Shaman.Common.Server.Messages;
using Shaman.Messages.General.Entity;
using Shaman.Router.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Common.Server.MM.Providers
{
    public interface IMatchMakerServerInfoProvider
    {
        void Start();
        void Stop();
        EntityDictionary<ServerInfo> GetGameServers();
        ServerInfo GetServer(int serverId);
        // Task ActualizeMe();
        ServerInfo GetLessLoadedServer();
        //
        // Task<Guid> CreateRoom(int serverId, Guid roomId, Dictionary<byte, object> properties,
        //     Dictionary<Guid, Dictionary<byte, object>> players);
        //
        // Task UpdateRoom(int serverId, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId);
    }
}