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
        Task<int> GetOnline();

        Task<List<MatchMakerConfiguration>> GetMmConfigurations(GameProject game, string version);
        Task<List<MatchMakerConfiguration>> GetAllMmConfigurations(GameProject game, bool actualOnly = true);
        Task CreateMmConfiguration(GameProject game, string name, string address, ushort port);
        Task UpdateMmConfiguration(GameProject game, string name, string address, ushort port);
        Task<List<Backend>> GetBackends();
        Task<EntityDictionary<ServerInfo>> GetAllServerInfo();
        Task<int?> GetServerId(ServerIdentity identity);
        Task<int> CreateServerInfo(ServerInfo serverInfo);
        Task UpdateServerInfoActualizedOn(int id, int peerCount, string name, string region, ushort httpPort, ushort httpsPort);
    }
}