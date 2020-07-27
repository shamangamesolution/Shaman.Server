using System.Collections.Generic;
using Shaman.Messages;
using Shaman.Messages.General.Entity.Router;
using Shaman.Serialization.Messages;

namespace Shaman.Router.Data.Providers
{
    public interface IRouterServerInfoProvider
    {
        void Start();
        void Stop();
        EntityDictionary<ServerInfo> GetAllServers();
        EntityDictionary<BundleInfo> GetAllBundles();
    }
}