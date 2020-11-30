using System;
using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;

namespace Sample.Shared.Data.Storage
{
    public abstract class StorageContainer : IStorageContainer
    {
        protected string ContainerVersion;
        protected bool IsUpdating = false;
        
        protected IStorageContainerUpdater ContainerUpdater;
        protected IShamanLogger Logger;
        protected ISerializer SerializerFactory;
        protected ITaskSchedulerFactory TaskSchedulerFactory;
        protected ITaskScheduler TaskScheduler;
        
        protected DataStorage Storage;
        protected StorageContainerStatus Status;
        protected Action<StorageContainerStatus> OnStorageUpdatedEvent;
        
        public StorageContainer(IStorageContainerUpdater containerUpdater, IShamanLogger logger, ISerializer serializerFactory, ITaskSchedulerFactory taskSchedulerFactory)
        {
            this.ContainerUpdater = containerUpdater;
            Logger = logger;
            SerializerFactory = serializerFactory;
            TaskSchedulerFactory = taskSchedulerFactory;

            TaskScheduler = TaskSchedulerFactory.GetTaskScheduler();
            Status = StorageContainerStatus.Idle;
        }
        
        #region Checkers
        public DataStorage GetStorage()
        {
            return Storage;
        }
        
        public bool IsReadyForRequests()
        {
            return (Storage != null) && !Storage.IsLocked() && !IsUpdating;
        }

        public abstract Task CheckUpdates();

        public void Start(string containerVersion)
        {
            ContainerVersion = containerVersion;
            TaskScheduler.ScheduleOnInterval(() => CheckUpdates(), 1000, 5000);
        }

        public Action<StorageContainerStatus> SubscribeOnStorageUpdated(Action<StorageContainerStatus> action)
        {
            return OnStorageUpdatedEvent += action;
        }

        protected void ChangeStatus(StorageContainerStatus newStatus)
        {
            Status = newStatus;
            OnStorageUpdatedEvent?.Invoke(newStatus);
        }
        
        #endregion
    }
}