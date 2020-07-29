using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Utils.Logging;

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