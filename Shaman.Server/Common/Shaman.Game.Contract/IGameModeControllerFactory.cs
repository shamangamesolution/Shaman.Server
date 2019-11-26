using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Game.Contract
{
    public interface IGameModeControllerFactory
    {
//        IGameModeController GetGameModeController(GameMode mode, IRoom room, ITaskScheduler taskScheduler, IRoomPropertiesContainer roomPropertiesContainer, League league);
        IGameModeController GetGameModeController(IRoom room, ITaskScheduler taskScheduler, IRoomPropertiesContainer roomPropertiesContainer);
    }
}