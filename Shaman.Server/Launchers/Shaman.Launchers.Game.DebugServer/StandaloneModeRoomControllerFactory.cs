using System;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;

namespace Shaman.Launchers.Game.DebugServer
{
    /// <summary>
    /// This implementation creates room controller factory using bundle which was got from standalone launcher
    /// </summary>
    public class StandaloneModeRoomControllerFactory : IRoomControllerFactory
    {
        private IGameBundle _bundle;
        private IRoomControllerFactory _roomControllerFactory;

        public void Initialize(IShamanComponents shamanComponents)
        {
            // note, avoid construction inside ctr - dependencies should be built first.
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
            if (_roomControllerFactory == null)
            {
                throw new NullReferenceException("StandaloneModeRoomControllerFactory wasn't initialized");
            }
            return _roomControllerFactory.GetGameModeController(room, taskScheduler,
                roomPropertiesContainer);
        }
    }
}