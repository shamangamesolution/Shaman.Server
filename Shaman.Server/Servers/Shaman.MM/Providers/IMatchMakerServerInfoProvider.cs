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

        Guid CreateRoom(int serverId, Dictionary<byte, object> properties,
            Dictionary<Guid, Dictionary<byte, object>> players);

        void UpdateRoom(int serverId, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId);
        Task<string> GetBundleUri();
    }
}