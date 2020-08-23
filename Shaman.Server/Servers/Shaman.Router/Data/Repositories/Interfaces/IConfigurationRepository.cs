using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Server.Messages;
using Shaman.Router.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Router.Data.Repositories.Interfaces
{
    public interface IConfigurationRepository
    {
        Task<EntityDictionary<ServerInfo>> GetAllServerInfo();
        Task<List<int>> GetServerId(ServerIdentity identity);
        Task<int> CreateServerInfo(ServerInfo serverInfo);
        Task UpdateServerInfoActualizedOn(int id, int peerCount, ushort httpPort, ushort httpsPort);
        Task<EntityDictionary<BundleInfo>> GetBundlesInfo();
    }
}