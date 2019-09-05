namespace Shaman.Common.Utils.TaskScheduling
{
    public interface ITaskSchedulerFactory
    {
        ITaskScheduler GetTaskScheduler();
    }
}