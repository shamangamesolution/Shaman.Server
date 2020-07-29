using System;
using Shaman.Common.Contract;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;

namespace Shaman.Game
{
    public class StandaloneModeRoomControllerFactory : IRoomControllerFactory
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IGameBundle _bundle;
        private readonly IRoomControllerFactory _roomControllerFactory;

        public StandaloneModeRoomControllerFactory(IShamanComponents shamanComponents)
        {
            _bundle = StandaloneServerLauncher.StandaloneBundle;
            _bundle.OnInitialize(shamanComponents);
            _roomControllerFactory = _bundle.GetRoomControllerFactory();
            if (_roomControllerFactory == null)
            {
                throw new NullReferenceException("Game bundle returned null factory");
            }
        }

        public IRoomController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return _roomControllerFactory.GetGameModeController(room, taskScheduler,
                roomPropertiesContainer);
        }
    }
}