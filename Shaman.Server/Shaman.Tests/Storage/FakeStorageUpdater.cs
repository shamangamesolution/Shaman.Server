using System.Threading.Tasks;
using Shaman.Messages.General.Entity.Storage;

namespace Shaman.Tests.Storage
{
    public class FakeStorageUpdater : IStorageContainerUpdater
    {
        
        public FakeStorageUpdater()
        {

        }


        public async Task<string> GetDatabaseVersion()
        {
            return "fake.1.0.0";
        }

        public async Task<DataStorage> GetStorage()
        {
            return new DataStorage()
            {
            };
        }
    }
}