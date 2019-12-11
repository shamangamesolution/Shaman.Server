using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Servers;
using Shaman.Messages;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Router;
using Shaman.Messages.MM;

namespace Shaman.Router.Data.Repositories.Interfaces
{
    public interface IConfigurationRepository
    {
        Task<EntityDictionary<ServerInfo>> GetAllServerInfo();
        Task<int?> GetServerId(ServerIdentity identity);
        Task<int> CreateServerInfo(ServerInfo serverInfo);
        Task UpdateServerInfoActualizedOn(int id, int peerCount, string name, string region, ushort httpPort, ushort httpsPort);
        Task<EntityDictionary<BundleInfo>> GetBundlesInfo();
    }
}