using System.Threading.Tasks;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Routing.Common.Actualization;

namespace Shaman.Routing.Balancing.Client
{
    public class RouterServerActualizer : IServerActualizer
    {
        private readonly IApplicationConfig _config;
        private readonly IRouterClient _routerClient;
        private readonly IStatisticsProvider _statsProvider;
        private readonly ITaskScheduler _taskScheduler;
        
        private IPendingTask _actualizeTask;
        
        public RouterServerActualizer(IApplicationConfig config, IRouterClient routerClient, IStatisticsProvider statsProvider, ITaskSchedulerFactory taskSchedulerFactory)
        {
            _config = config;
            _routerClient = routerClient;
            _statsProvider = statsProvider;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
        }

        public async Task Actualize(int peersCount)
        {
            await _routerClient.Actualize(GetServerIdentity(), _config.ServerName, _config.Region, peersCount,
                _config.BindToPortHttp);
        }

        public void Start(int actualizationPeriodMs)
        {
            _actualizeTask = _taskScheduler.ScheduleOnInterval(async () =>
             {
                 //actualize
                 await Actualize(_statsProvider.GetPeerCount());
             }, 0, actualizationPeriodMs);
        }

        public void Stop()
        {
            _taskScheduler.Remove(_actualizeTask);
        }

        private ServerIdentity GetServerIdentity()
        {
            return new ServerIdentity(_config.PublicDomainNameOrAddress,
                _config.ListenPorts, _config.ServerRole);
        }
    }
}