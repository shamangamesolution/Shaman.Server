using System;
using System.Threading.Tasks;
using Sample.Shared.Data.DTO.Requests;
using Sample.Shared.Data.DTO.Responses;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;

namespace Code.Network
{
    public class ClientStorageContainer : StorageContainer
    {
        private object _syncRoot = new object();
        private bool _isLoadingData = false;
        private IRequestSender _requestSender;
        private string _backendUrl;
        
        public ClientStorageContainer(IRequestSender requestSender, IShamanLogger logger, ISerializer serializerFactory, ITaskSchedulerFactory taskSchedulerFactory) 
            : base(null, logger, serializerFactory, taskSchedulerFactory)
        {
            _requestSender = requestSender;
        }

        public void Initialize(string backendUrl)
        {
            _backendUrl = backendUrl;
        }
    
        public async void UpdateOnce()
        {
            if (_isLoadingData)
                return;
        
            try
            {
                ChangeStatus(StorageContainerStatus.CheckingUpdates);
                var result = await _requestSender.SendRequest<GetCurrentStorageVersionResponse>(_backendUrl,
                    new GetCurrentStorageVersionRequest());
                OnGetVersionResponse(result);
            }           
            catch (Exception ex)
            {
                IsUpdating = false;
                _isLoadingData = false;
                Logger.Error($"Storage update failed: {ex}");
                ChangeStatus(StorageContainerStatus.OperationFailed);
            }
        }

        private void OnGetStorageResponse(GetStorageHttpResponse response)
        {
            if (!response.Success)
            {
                Logger.Error($"Error getting storage: {response.Message}");
                ChangeStatus(StorageContainerStatus.OperationFailed);
                return;
            }
        
            var storage = SerializerFactory.DeserializeAs<DataStorage>(CompressHelper.Decompress(response.SerializedAndCompressedStorage));

        
            lock (_syncRoot)
            {
                Storage = new DataStorage();
                IsUpdating = true;
                Storage.InitStorage(storage);
            }

            Logger.Info($"Storage {Storage.DatabaseVersion} loaded to cache");
            ChangeStatus(StorageContainerStatus.Updated);

            IsUpdating = false;
            _isLoadingData = false;
        }
    
        private async void OnGetVersionResponse(GetCurrentStorageVersionResponse response)
        {
            if (!response.Success)
            {
                Logger.Error($"Error getting storage version: {response.Message}");
                ChangeStatus(StorageContainerStatus.OperationFailed);
                return;
            }
                    
            if (Storage == null || response.CurrentDatabaseVersion != Storage.DatabaseVersion || response.CurrentBackendVersion != Storage.ServerVersion)
            {
                _isLoadingData = true;
                Logger.Info($"Starting storage update to version {response.CurrentDatabaseVersion}");
            
                var result = await _requestSender.SendRequest<GetStorageHttpResponse>(_backendUrl,
                    new GetStorageHttpRequest());
                OnGetStorageResponse(result);
            }
        }

        public override async Task CheckUpdates()
        {
            return;
        }
    }
}
