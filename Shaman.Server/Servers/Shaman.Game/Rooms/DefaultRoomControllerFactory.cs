using System;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;

namespace Shaman.Game.Rooms
{
    /// <summary>
    /// This implementation creates room controller factory using bundle passed through ctor
    /// </summary>
    public class DefaultRoomControllerFactory : IBundledRoomControllerFactory
    {
        private IRoomControllerFactory _bundledRoomControllerFactory;

        public void RegisterBundleRoomController(IRoomControllerFactory roomControllerFactory)
        {
            _bundledRoomControllerFactory = roomControllerFactory;
        }
        
        public IRoomController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            if (_bundledRoomControllerFactory == null)
                throw new NullReferenceException("Bundle's room factory does not registered");
            var roomController = _bundledRoomControllerFactory.GetGameModeController(room, taskScheduler,
                roomPropertiesContainer);
            if (roomController == null)
                throw new NullReferenceException("Bundle's room controller is null");
            return roomController;
        }
    }
}