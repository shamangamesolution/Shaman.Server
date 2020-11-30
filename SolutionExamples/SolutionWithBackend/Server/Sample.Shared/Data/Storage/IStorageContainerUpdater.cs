using System.Threading.Tasks;

namespace Sample.Shared.Data.Storage
{
    public interface IStorageContainerUpdater
    {
        Task<string> GetDatabaseVersion();
        Task<DataStorage> GetStorage();
    }
}
