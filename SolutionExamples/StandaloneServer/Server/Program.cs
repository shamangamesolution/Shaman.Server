using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game;
using Shaman.Game.Contract;

namespace Server
{
    class GameControllerFactory : IGameModeControllerFactory
    {
        private readonly ISerializer _serializer;

        public GameControllerFactory(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public IGameModeController GetGameModeController(IRoom room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new GameController(room, roomPropertiesContainer, _serializer);
        }
    }

    class GameBundle : IGameBundle
    {
        private IShamanComponents _shamanComponents;

        public IGameModeControllerFactory GetGameModeControllerFactory()
        {
            return new GameControllerFactory(_shamanComponents.Serializer);
        }

        public void OnInitialize(IShamanComponents shamanComponents)
        {
            _shamanComponents = shamanComponents;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            StandaloneServerLauncher.Launch(new GameBundle(), args, "TestGame", "SomeRegion", "localhost",
                new List<ushort> {23453}, 7005);
        }
    }
}