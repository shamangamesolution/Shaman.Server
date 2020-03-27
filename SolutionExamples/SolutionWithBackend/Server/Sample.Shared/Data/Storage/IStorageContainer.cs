using System;

namespace Sample.Shared.Data.Storage
{
    public enum StorageContainerStatus
    {
        Idle = 1,
        CheckingUpdates = 2,
        Updated = 3,
        OperationFailed = 4
    }
    public interface IStorageContainer
    {
        DataStorage GetStorage();
        bool IsReadyForRequests();
        void Start(string containerVersion);
        Action<StorageContainerStatus> SubscribeOnStorageUpdated(Action<StorageContainerStatus> action);
    }
}