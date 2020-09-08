using Shaman.Bundling.Common;
using Shaman.Contract.Routing;
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