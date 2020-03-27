using System;
using System.Threading.Tasks;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;

namespace Sample.BackEnd.Data.Containers
{
    public class BackendStorageContainer : StorageContainer
    {
        private object _syncRoot = new object();
        private bool _isLoadingData = false;

        public BackendStorageContainer(IStorageContainerUpdater containerUpdater, IShamanLogger logger, ISerializer serializerFactory, ITaskSchedulerFactory taskSchedulerFactory) 
            : base(containerUpdater, logger, serializerFactory, taskSchedulerFactory)
        {
        }

        public override async Task CheckUpdates()
        {
            if (_isLoadingData)
                return;

            try
            {
                ChangeStatus(StorageContainerStatus.CheckingUpdates);
                var currentVersion = (await ContainerUpdater.GetDatabaseVersion());

                if (Storage == null || currentVersion != Storage.DatabaseVersion)
                {
                    _isLoadingData = true;
                    Logger.Info($"Starting storage update to version {currentVersion}");
                    var storage = await ContainerUpdater.GetStorage();
                    
                    //check copnsistency
                    if (!storage.ConsistencyCheck())
                        throw new Exception($"StorageUpdater error: storage is not consistent");
                    
                    //serialization chaeck
                    Logger.Info($"Starting storage serialization check");
                    SerializerFactory.DeserializeAs<DataStorage>(SerializerFactory.Serialize(storage));
                    //EntityBase.DeserializeAs<DataStorage>(SerializerFactory, storage.Serialize(SerializerFactory));
                    
                    storage.DatabaseVersion = currentVersion;//(await _tempRepo.GetVersion(VersionType.DataBase)).ToString();
                    storage.ServerVersion = ContainerVersion;
                    
                    lock (_syncRoot)
                    {
                        Storage = new DataStorage(Logger);
                        IsUpdating = true;
                        Storage.InitStorage(storage);
                    }
                    ChangeStatus(StorageContainerStatus.Updated);
                    Logger.Info($"Storage {Storage.DatabaseVersion} loaded to cache");
                    IsUpdating = false;
                    _isLoadingData = false;
                }
            }           
            catch (Exception ex)
            {
                IsUpdating = false;
                _isLoadingData = false;
                Logger.Error($"Storage update failed: {ex}");
                ChangeStatus(StorageContainerStatus.OperationFailed);
            }
        }
    }
}