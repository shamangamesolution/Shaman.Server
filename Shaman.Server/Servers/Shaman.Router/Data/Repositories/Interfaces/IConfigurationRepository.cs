using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Router;

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
    }
}