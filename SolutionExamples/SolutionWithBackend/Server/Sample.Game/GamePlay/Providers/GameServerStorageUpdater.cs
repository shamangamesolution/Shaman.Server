using System.Threading.Tasks;
using Sample.Shared.Data.DTO.Requests;
using Sample.Shared.Data.DTO.Responses;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Game.Contract;

namespace Sample.Game.GamePlay.Providers
{
    public class GameServerStorageUpdater : IStorageContainerUpdater
    {
        private IRequestSender _requestSender;
        private IShamanLogger _logger;
        private IBackendProvider _backendProvider;
        private ISerializer _serializerFactory;
        
        public GameServerStorageUpdater(IRequestSender requestSender, IShamanLogger logger, IBackendProvider backendProvider, ISerializer serializerFactory)
        {
            _requestSender = requestSender;
            _logger = logger;
            _backendProvider = backendProvider;
            _serializerFactory = serializerFactory;
        }


        public async Task<string> GetDatabaseVersion()
        {
            var response = await _requestSender.SendRequest<GetCurrentStorageVersionResponse>(
                _backendProvider.GetFirstBackendUrl(),
                new GetCurrentStorageVersionRequest());
            
            if (!response.Success)
            {
                _logger.Error($"Error requesting database version: {response.Message}");
                return null;
            }

            return response.CurrentDatabaseVersion;
        }

        public async Task<DataStorage> GetStorage()
        {
            var response = await _requestSender.SendRequest<GetStorageHttpResponse>(
                _backendProvider.GetFirstBackendUrl(),
                new GetStorageHttpRequest());
            
            if (!response.Success)
            {
                _logger.Error($"Error requesting storage: {response.Message}");
                return null;
            }

            return _serializerFactory.DeserializeAs<DataStorage>(CompressHelper.Decompress(response.SerializedAndCompressedStorage));         
        }
    }
}