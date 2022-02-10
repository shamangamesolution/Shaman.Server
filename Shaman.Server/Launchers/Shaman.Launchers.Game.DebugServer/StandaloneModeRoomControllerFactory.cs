using System;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;
using Shaman.Game.Rooms;

namespace Shaman.Launchers.Game.DebugServer
{
    /// <summary>
    /// This implementation creates room controller factory using bundle which was got from standalone launcher
    /// </summary>
    public class StandaloneModeRoomControllerFactory : IBundledRoomControllerFactory
    {
        private IGameBundle _bundle;
        private IRoomControllerFactory _roomControllerFactory;

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

        public void RegisterBundleRoomController(IRoomControllerFactory roomControllerFactory)
        {
            _roomControllerFactory = roomControllerFactory;
        }
    }
}