using Shaman.Common.Contract;

namespace Shaman.Common.Utils.TaskScheduling
{
    public interface ITaskSchedulerFactory
    {
        ITaskScheduler GetTaskScheduler();
    }
}