using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;

namespace Server
{
    public class MyGameControllerFactory : IGameModeControllerFactory
    {
        private readonly ISerializer _serializer;

        public MyGameControllerFactory(ISerializer serializer)
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