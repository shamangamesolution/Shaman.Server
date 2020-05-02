using System;
using Sample.Shared.Data.Entity.Gameplay;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Messages;

namespace Sample.Game.GamePlay.Controllers
{
    public class SampleGameModeControllerFactory : IGameModeControllerFactory
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly IBackendProvider _backendProvider;
        private readonly IStorageContainer _storageContainer;
        private readonly ISerializer _serializerFactory;


        public SampleGameModeControllerFactory(
            IRequestSender requestSender, 
            IShamanLogger logger, 
            IBackendProvider backendProvider, 
            IStorageContainer storageContainer, 
            ISerializer serializerFactory)
        {
            this._requestSender = requestSender;
            this._logger = logger;
            this._backendProvider = backendProvider;
            this._storageContainer = storageContainer;
            _serializerFactory = serializerFactory;
        }

        public IGameModeController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            if (!roomPropertiesContainer.IsRoomPropertiesContainsKey(PropertyCode.RoomProperties.GameMode))
                throw new Exception($"GameModeControllerFactory.GetGameModeController error: no GameMode in property container");
            var mode = (GameMode)roomPropertiesContainer.GetRoomProperty<byte>(PropertyCode.RoomProperties.GameMode).Value;
            
            switch(mode)
            {
                default:
                    return new TestModeController(room, _logger, _requestSender, taskScheduler, _storageContainer, roomPropertiesContainer, _backendProvider, _serializerFactory);        
                
            }
        }
    }
}