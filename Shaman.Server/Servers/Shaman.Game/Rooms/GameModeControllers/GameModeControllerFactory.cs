using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;
using Shaman.ServerSharedUtilities.Backends;

namespace Shaman.Game.Rooms.GameModeControllers
{
    public class MsGameModeControllerFactory : IGameModeControllerFactory
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly IBackendProvider _backendProvider;
        private readonly IStorageContainer _storageContainer;
        private readonly ISerializerFactory _serializerFactory;
        private readonly IRoomPropertiesContainer _roomPropertiesContainer;
        
        public MsGameModeControllerFactory(
            IRequestSender requestSender, 
            IShamanLogger logger, 
            IBackendProvider backendProvider, 
            IStorageContainer storageContainer, 
            ISerializerFactory serializerFactory, 
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            this._requestSender = requestSender;
            this._logger = logger;
            this._backendProvider = backendProvider;
            this._storageContainer = storageContainer;
            this._serializerFactory = serializerFactory;
            this._roomPropertiesContainer = roomPropertiesContainer;
        }
        
        public IGameModeController GetGameModeController(GameMode mode, IRoom room, ITaskScheduler taskScheduler)
        {
            switch(mode)
            {
                default:
                    return new TestModeController(room);        
                
            }
        }
    }
}
