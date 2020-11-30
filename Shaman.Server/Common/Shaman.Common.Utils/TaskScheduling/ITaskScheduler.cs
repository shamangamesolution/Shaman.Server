using System;
using System.Threading.Tasks;

namespace Shaman.Common.Utils.TaskScheduling
{
    public interface ITaskScheduler : IDisposable
    {
        PendingTask Schedule(Action action, long firstInMs);
        PendingTask Schedule(Func<Task> action, long firstInMs);
        PendingTask ScheduleOnInterval(Action action, long firstInMs, long regularInMs, bool shortLiving = false);
        PendingTask ScheduleOnInterval(Func<Task> action, long firstInMs, long regularInMs, bool shortLiving = false);
        void ScheduleOnceOnNow(Action action);
        void Remove(PendingTask task);
        void RemoveAll();
        void ScheduleOnceOnNow(Func<Task> action);
    }
}