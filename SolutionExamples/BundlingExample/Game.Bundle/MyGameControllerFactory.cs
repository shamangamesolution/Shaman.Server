using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;

namespace Game.Bundle
{
    public class MyGameControllerFactory : IGameModeControllerFactory
    {
        private readonly ISerializer _serializer;

        //todo most of injected types should be passed through method GetGameModeController
        public MyGameControllerFactory(IRequestSender requestSender, IShamanLogger logger,
            IBackendProvider backendProvider, ISerializer serializer)
        {
            _serializer = serializer;
        }

        public IGameModeController GetGameModeController(IRoom room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new MyGameController(_serializer);
        }
    }
}