using System.Threading.Tasks;
using Sample.Shared.Data.Storage;

namespace Sample.BackEnd.Data.Repositories.Interfaces
{
    public interface IStorageRepository
    {
        Task<DataStorage> GetStorage();
    }
}