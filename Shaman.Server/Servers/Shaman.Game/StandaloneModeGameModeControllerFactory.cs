using System;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;

namespace Shaman.Game
{
    public class StandaloneModeGameModeControllerFactory : IGameModeControllerFactory
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IGameBundle _bundle;
        private readonly IGameModeControllerFactory _gameModeControllerFactory;

        public StandaloneModeGameModeControllerFactory(IShamanComponents shamanComponents)
        {
            _bundle = StandaloneServerLauncher.StandaloneBundle;
            _bundle.OnInitialize(shamanComponents);
            _gameModeControllerFactory = _bundle.GetGameModeControllerFactory();
            if (_gameModeControllerFactory == null)
            {
                throw new NullReferenceException("Game bundle returned null factory");
            }
        }

        public IGameModeController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return _gameModeControllerFactory.GetGameModeController(room, taskScheduler,
                roomPropertiesContainer);
        }
    }
}