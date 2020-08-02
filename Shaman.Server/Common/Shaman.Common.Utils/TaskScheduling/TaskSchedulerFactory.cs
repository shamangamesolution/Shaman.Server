using Shaman.Common.Utils.Logging;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Utils.TaskScheduling
{
    public class TaskSchedulerFactory : ITaskSchedulerFactory
    {
        private IShamanLogger _logger;
        
        public TaskSchedulerFactory(IShamanLogger logger)
        {
            _logger = logger;
        }
        
        public ITaskScheduler GetTaskScheduler()
        {
            return new TaskScheduler(_logger);
        }
    }
}