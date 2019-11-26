using System.Collections.Generic;
using Shaman.Messages;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.Router.Data.Providers
{
    public interface IRouterServerInfoProvider
    {
        void Start();
        void Stop();
        EntityDictionary<ServerInfo> GetAllServers(); 
    }
}