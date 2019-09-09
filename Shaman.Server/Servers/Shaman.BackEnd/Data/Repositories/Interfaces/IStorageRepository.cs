using System.Threading.Tasks;
using Shaman.Messages.General.Entity.Storage;

namespace Shaman.BackEnd.Data.Repositories.Interfaces
{
    public interface IStorageRepository
    {
        Task<DataStorage> GetStorage();
    }
}