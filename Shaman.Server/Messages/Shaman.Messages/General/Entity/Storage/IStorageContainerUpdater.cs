using System.Threading.Tasks;

namespace Shaman.Messages.General.Entity.Storage
{
    public interface IStorageContainerUpdater
    {
        Task<string> GetDatabaseVersion();
        Task<DataStorage> GetStorage();
    }
}
