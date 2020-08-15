namespace Shaman.Contract.Common
{
    public interface ITaskSchedulerFactory
    {
        ITaskScheduler GetTaskScheduler();
    }
}