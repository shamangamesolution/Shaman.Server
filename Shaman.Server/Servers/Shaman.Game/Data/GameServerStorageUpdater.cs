using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.DTO.Requests.Storage;
using Shaman.Messages.General.DTO.Responses.Storage;
using Shaman.Messages.General.Entity.Storage;
using Shaman.ServerSharedUtilities.Backends;

namespace Shaman.Game.Data
{
    public class GameServerStorageUpdater : IStorageContainerUpdater
    {
        private IRequestSender _requestSender;
        private IShamanLogger _logger;
        private IBackendProvider _backendProvider;
        private ISerializerFactory _serializerFactory;
        
        public GameServerStorageUpdater(IRequestSender requestSender, IShamanLogger logger, IBackendProvider backendProvider, ISerializerFactory serializerFactory)
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
            var response = await _requestSender.SendRequest<GetNotCompressedStorageResponse>(
                _backendProvider.GetFirstBackendUrl(),
                new GetNotCompressedStorageRequest());
            
            if (!response.Success)
            {
                _logger.Error($"Error requesting storage: {response.Message}");
                return null;
            }

            return EntityBase.DeserializeAs<DataStorage>(_serializerFactory, response.SerializedStorage); 
        }
    }
}