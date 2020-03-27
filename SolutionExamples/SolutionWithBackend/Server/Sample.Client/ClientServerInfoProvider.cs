using System;
using System.Collections.Generic;
using Shaman.Messages;
using Shaman.Messages.General.Entity.Router;

namespace Sample.Client
{
    public interface IGameServerInfoProvider
    {
        
        EntityDictionary<ServerInfo> GetGameServers();
        ServerInfo GetLessLoadedServer();

        Guid CreateRoom(int serverId, Dictionary<byte, object> properties,
            Dictionary<Guid, Dictionary<byte, object>> players);

        void UpdateRoom(int serverId, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId);
    }
    
    public class ClientServerInfoProvider
    {
        
    }
}