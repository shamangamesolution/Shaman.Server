using System;
using System.Threading.Tasks;

namespace Shaman.Contract.Common
{
    public interface ITaskScheduler : IDisposable
    {
        IPendingTask Schedule(Action action, long firstInMs);
        IPendingTask Schedule(Func<Task> action, long firstInMs);
        IPendingTask ScheduleOnInterval(Action action, long firstInMs, long regularInMs, bool shortLiving = false);
        IPendingTask ScheduleOnInterval(Func<Task> action, long firstInMs, long regularInMs, bool shortLiving = false);
        void ScheduleOnceOnNow(Action action);
        void Remove(IPendingTask task);
        void RemoveAll();
        void ScheduleOnceOnNow(Func<Task> action);
    }
}