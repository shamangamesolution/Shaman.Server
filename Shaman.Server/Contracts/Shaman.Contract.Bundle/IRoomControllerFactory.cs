using Shaman.Contract.Common;

namespace Shaman.Contract.Bundle
{
    /// <summary>
    /// Creates room controllers
    /// </summary>
    public interface IRoomControllerFactory
    {
        IRoomController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler, IRoomPropertiesContainer roomPropertiesContainer);
    }
}