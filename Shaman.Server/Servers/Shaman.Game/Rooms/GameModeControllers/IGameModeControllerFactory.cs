using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages.General.Entity;

namespace Shaman.Game.Rooms.GameModeControllers
{
    public interface IGameModeControllerFactory
    {
        IGameModeController GetGameModeController(GameMode mode, IRoom room, ITaskScheduler taskScheduler);
    }
}