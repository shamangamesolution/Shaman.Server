using System;

namespace Shaman.Common.Utils.TaskScheduling
{
    public interface ITaskScheduler
    {
        PendingTask Schedule(Action action, long firstInMs);
        PendingTask ScheduleOnInterval(Action action, long firstInMs, long regularInMs);
        void ScheduleOnceOnNow(Action action);

        void Remove(Guid taskId);
        void RemoveAll();
    }
}