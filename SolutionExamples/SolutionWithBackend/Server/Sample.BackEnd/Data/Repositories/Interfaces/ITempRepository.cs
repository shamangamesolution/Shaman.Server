using System.Threading.Tasks;
using Shaman.Messages.General.Entity;

namespace Sample.BackEnd.Data.Repositories.Interfaces
{
    public interface ITempRepository
    {
        Task<CustomVersion> GetVersion(VersionType dataBase);
        Task<CustomVersion> IncrementVersion(VersionType type, VersionComponent component);
    }
}