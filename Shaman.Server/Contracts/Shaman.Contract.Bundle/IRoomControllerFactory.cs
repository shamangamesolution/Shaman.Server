using Shaman.Contract.Common;

namespace Shaman.Contract.Bundle
{
    public interface IRoomControllerFactory
    {
        IRoomController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler, IRoomPropertiesContainer roomPropertiesContainer);
    }
}