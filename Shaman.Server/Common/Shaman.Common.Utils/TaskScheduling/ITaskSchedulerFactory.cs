using Shaman.Contract.Common;

namespace Shaman.Common.Utils.TaskScheduling
{
    public interface ITaskSchedulerFactory
    {
        ITaskScheduler GetTaskScheduler();
    }
}