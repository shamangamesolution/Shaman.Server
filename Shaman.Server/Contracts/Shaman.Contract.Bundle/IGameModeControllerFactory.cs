using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Contract.Bundle
{
    public interface IGameModeControllerFactory
    {
//        IGameModeController GetGameModeController(GameMode mode, IRoom room, ITaskScheduler taskScheduler, IRoomPropertiesContainer roomPropertiesContainer, League league);
        IGameModeController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler, IRoomPropertiesContainer roomPropertiesContainer);
    }
}