using System.Threading.Tasks;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.Shared.Data.Storage;
using Shaman.Messages.General.Entity;

namespace Sample.BackEnd.Data.Containers
{
    public class BackendStorageUpdater : IStorageContainerUpdater
    {
        private ITempRepository _tempRepo;
        private IStorageRepository _storageRepo;

        public BackendStorageUpdater(IStorageRepository storageRepo, ITempRepository tempRepo)
        {
            _storageRepo = storageRepo;
            _tempRepo = tempRepo;
        }

        public async Task<string> GetDatabaseVersion()
        {
            return (await _tempRepo.GetVersion(VersionType.DataBase)).ToString();
        }

        public async Task<DataStorage> GetStorage()
        {
            return await _storageRepo.GetStorage();
        }
    }
}