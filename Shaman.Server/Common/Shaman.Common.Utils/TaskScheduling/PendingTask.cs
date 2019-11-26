using System;
using System.Threading;
using Shaman.Common.Utils.Logging;

namespace Shaman.Common.Utils.TaskScheduling
{
    public class PendingTask : IDisposable
    {
        private readonly long _firstIntervalInMs;
        private readonly long _intervalInMs;
        private Action _action;
        private Timer _timer;
        private bool _cancelled;
        private readonly IShamanLogger _logger;
        private readonly bool _shortLiving;
        private readonly DateTime _creatingTime;
        private bool _goneToEnd;
        private static TimeSpan _shortLivingTaskMaxDuration = TimeSpan.FromMinutes(15);
        private readonly bool _isPeriodic;
        
        private static int _activeTimersCount;
        private static int _activePeriodicTimersCount;
        private static int _activePeriodicSlTimersCount;
        private static int _executingActionsCount;

        public static int GetActiveTimersCount()
        {
            return _activeTimersCount;
        }
        public static int GetActivePeriodicTimersCount()
        {
            return _activePeriodicTimersCount;
        }
        public static int GetActivePeriodicSlTimersCount()
        {
            return _activePeriodicSlTimersCount;
        }
        public static int GetExecutingActionsCount()
        {
            return _executingActionsCount;
        }

        public static void DurationMonitoringTime(TimeSpan newDuration)
        {
            _shortLivingTaskMaxDuration = newDuration;
        }

        public PendingTask(Action action, long firstIntervalInMs, long intervalInMs, IShamanLogger logger, bool shortLiving = false)
        {
            this._action = action;
            this._firstIntervalInMs = firstIntervalInMs;
            this._intervalInMs = intervalInMs;
            this._logger = logger;
            _shortLiving = shortLiving;
            _isPeriodic = intervalInMs != Timeout.Infinite;
            if (_shortLiving)
            {
                _creatingTime = DateTime.UtcNow;
            }
        }

        public bool IsCompleted
        {
            get { return  _cancelled || (this._intervalInMs == Timeout.Infinite && _goneToEnd); }
        }
        
        public void Schedule()
        {
            if (_isPeriodic)
                if (_shortLiving)
                    Interlocked.Increment(ref _activePeriodicSlTimersCount);
                else
                    Interlocked.Increment(ref _activePeriodicTimersCount);
            else
                Interlocked.Increment(ref _activeTimersCount);    
            
            _timer = new Timer((TimerCallback) (x =>
            {
                if (_shortLiving)
                {
                    if ((DateTime.UtcNow - _creatingTime) > _shortLivingTaskMaxDuration )
                    {
                        _logger?.Error(
                            $"SHORT-LIVING TASK LIVES TOO LONG. Declaring type: '{_action?.Method?.DeclaringType?.FullName}', Method name: '{_action?.Method?.Name} ({DateTime.UtcNow - _creatingTime})'. TASK WILL BE SELF DESTROYED!");
                        Dispose();
                        return;
                    }
                }
                
                if (_cancelled)
                {
                    return;
                }

                if (_action == null)
                {
                    _logger?.Error($"Action is null");
                    _cancelled = true;
                    return;
                }

                try
                {
                    Interlocked.Increment(ref _executingActionsCount);
                    _action();
                }
                catch (Exception ex)
                {
                    _logger?.Error($"Task {_action?.Target} executing error: {ex}");
                }
                finally
                {
                    Interlocked.Decrement(ref _executingActionsCount);
                    _goneToEnd = true;
                }
            }), (object) null, _firstIntervalInMs, _intervalInMs);
        }

        public virtual void Dispose()
        {
            if (_isPeriodic)
                if (_shortLiving)
                    Interlocked.Decrement(ref _activePeriodicSlTimersCount);
                else
                    Interlocked.Decrement(ref _activePeriodicTimersCount);
            else
                Interlocked.Decrement(ref _activeTimersCount);
            this._cancelled = true;
            this._action = (Action) null;
            try
            {
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                _timer?.Dispose();
            }
            catch (Exception e)
            {
                _logger?.Error($"PendingTask dispose exception: {e}, {e.InnerException}");
            }
        }       
        
        public string GetActionName()
        {
            if (_action == null)
                return "";
            return _action.Method.Name;
        }
    }
}