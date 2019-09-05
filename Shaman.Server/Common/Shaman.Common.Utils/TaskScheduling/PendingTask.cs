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
        private IShamanLogger _logger;
        public Guid Id;
        private bool _goneToEnd;
        public PendingTask(Action action, long firstIntervalInMs, long intervalInMs, IShamanLogger logger)
        {
            this._action = action;
            this._firstIntervalInMs = firstIntervalInMs;
            this._intervalInMs = intervalInMs;
            this._logger = logger;
            this.Id = Guid.NewGuid();
        }

        public bool IsCompleted
        {
            get { return _cancelled || (this._intervalInMs == Timeout.Infinite && _goneToEnd); }
        }
        
        public void Schedule()
        {
            _timer = new Timer((TimerCallback) (x =>
            {
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
                    _action();
                }
                catch (Exception ex)
                {
                    _logger?.Error($"Task {_action?.Target} executing error: {ex}");
                }
                finally
                {
                    _goneToEnd = true;
                }
            }), (object) null, _firstIntervalInMs, _intervalInMs);
        }

        public virtual void Dispose()
        {
            this._cancelled = true;
            this._action = (Action) null;
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
        }       
        
        public string GetActionName()
        {
            if (_action == null)
                return "";
            return _action.Method.Name;
        }
    }
}