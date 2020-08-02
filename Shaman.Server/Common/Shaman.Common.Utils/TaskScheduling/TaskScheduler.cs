using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Utils.TaskScheduling
{
    public class TaskScheduler : ITaskScheduler
    {
        private readonly List<PendingTask> _tasks = new List<PendingTask>();
        private readonly object _listLock = new object();
        private readonly IShamanLogger _logger;

        private static int _scheduledOnceTasks;

        public static int GetGlobalScheduledOnceTasksCount()
        {
            return _scheduledOnceTasks;
        }
        
        public TaskScheduler(IShamanLogger logger)
        {
            _logger = logger;
            ScheduleOnInterval(() =>
            {
                lock (_listLock)
                {
                    _tasks.Where(t => t.IsCompleted).ToList().ForEach(t => t.Dispose());
                    _tasks.RemoveAll(t => t.IsCompleted);
                }
            }, 0, 1000);
        }

        public IPendingTask Schedule(Action action, long firstInMs)
        {
            return ScheduleOnInterval(action, firstInMs, Timeout.Infinite, true);
        }
        public IPendingTask Schedule(Func<Task> action, long firstInMs)
        {
            return ScheduleOnInterval(action, firstInMs, Timeout.Infinite, true);
        }

        public IPendingTask ScheduleOnInterval(Action action, long firstInMs, long regularInMs, bool shortLiving = false)
        {
            var pending = new PendingTask(action, firstInMs, regularInMs, _logger, shortLiving);
            pending.Schedule();
            lock (_listLock)
            {
                _tasks.Add(pending);
            }

            return pending;
        }
        public IPendingTask ScheduleOnInterval(Func<Task> action, long firstInMs, long regularInMs, bool shortLiving = false)
        {
            var pending = new PendingTask(async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception e)
                {
                    _logger?.Error($"ScheduleOnIntervalAsync task error: {e}");
                }
            }, firstInMs, regularInMs, _logger, shortLiving);
            pending.Schedule();
            lock (_listLock)
            {
                _tasks.Add(pending);
            }

            return pending;
        }

        public void ScheduleOnceOnNow(Action action)
        {
            Interlocked.Increment(ref _scheduledOnceTasks);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    _logger?.Error($"ScheduleOnceOnNow task error: {e}");
                }
                finally
                {
                    Interlocked.Decrement(ref _scheduledOnceTasks);
                }
            });
        }
        public void ScheduleOnceOnNow(Func<Task> action)
        {
            Interlocked.Increment(ref _scheduledOnceTasks);
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception e)
                {
                    _logger?.Error($"ScheduleOnceOnNowAsync task error: {e}");
                }
                finally
                {
                    Interlocked.Decrement(ref _scheduledOnceTasks);
                }
            });
        }

        public void Remove(IPendingTask task)
        {
            var pt = task as PendingTask; 
            if (pt == null)
                return;
            lock (_listLock)
            {
                var pendingTask = _tasks.FirstOrDefault(t => t == pt);
                if (pendingTask == null)
                    return;
                _logger?.Debug($"Removing {pendingTask.GetActionName()}");
                pendingTask.Dispose();
                _tasks.Remove(pt);
            }   
        }

        public void RemoveAll()
        {
            lock (_listLock)
            {
                foreach (var task in _tasks)
                {
                    task.Dispose();
                }
                _tasks.Clear();
            } 
        }

        public void Dispose()
        {
            RemoveAll();
        }
    }
}