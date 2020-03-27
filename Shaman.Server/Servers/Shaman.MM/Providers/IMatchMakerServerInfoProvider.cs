using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Messages;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.MM.Providers
{
    public interface IMatchMakerServerInfoProvider
    {
        void Start();
        void Stop();
        EntityDictionary<ServerInfo> GetGameServers();
        ServerInfo GetServer(int serverId);
        Task ActualizeMe();
        ServerInfo GetLessLoadedServer();

        Task<Guid> CreateRoom(int serverId, Guid roomId, Dictionary<byte, object> properties,
            Dictionary<Guid, Dictionary<byte, object>> players);

        Task UpdateRoom(int serverId, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId);
    }
}