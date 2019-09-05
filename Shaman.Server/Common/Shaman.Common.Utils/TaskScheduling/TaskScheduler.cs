using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Shaman.Common.Utils.Logging;

namespace Shaman.Common.Utils.TaskScheduling
{
    public class TaskScheduler : ITaskScheduler, IDisposable
    {
        private Guid _id;
        List<PendingTask> _tasks = new List<PendingTask>();
        private object _listLock = new object();
        private IShamanLogger _logger;
        private PendingTask _clearTask;
        
        public TaskScheduler(IShamanLogger logger)
        {
            _logger = logger;
            _id = Guid.NewGuid();
            _clearTask = ScheduleOnInterval(() =>
            {
                lock (_listLock)
                {
                    _tasks.Where(t => t.IsCompleted).ToList().ForEach(t => t.Dispose());
                    _tasks.RemoveAll(t => t.IsCompleted);
                }
            }, 0, 1000);
        }

        public PendingTask Schedule(Action action, long firstInMs)
        {
            return ScheduleOnInterval(action, firstInMs, Timeout.Infinite);
        }

        public PendingTask ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            _logger.Debug($"ScheduleOnInterval ({_id}) Scheduling: {action.Method.Name} to first in in {firstInMs} ms and repeat every {regularInMs} ms");
            _logger.Debug($"ScheduleOnInterval ({_id}) Tasks count: {_tasks.Count}");
            var pending = new PendingTask(action, firstInMs, regularInMs, _logger);
            pending.Schedule();
            lock (_listLock)
            {
                _tasks.Add(pending);
            }

            return pending;
        }

        public void ScheduleOnceOnNow(Action action)
        {
            ScheduleOnInterval(action, 0, Timeout.Infinite);
        }

        public void Remove(Guid taskId)
        {

            lock (_listLock)
            {
                var pendingTask = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (pendingTask == null)
                    return;
                _logger?.Debug($"Removing {pendingTask.GetActionName()}");
                pendingTask.Dispose();
                _tasks.RemoveAll(t => t.Id == taskId);
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
            lock (_listLock)
            {
                foreach (var item in _tasks)
                {
                   item.Dispose();
                }

                _tasks.Clear();
            }
        }
    }
}