using System.Threading.Tasks;
using Shaman.Common.Server.Actualization;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Common.Server.Providers;
using Shaman.Contract.Common;
using Shaman.Router.Messages;

namespace Shaman.Router.Client
{
    public class RouterServerActualizer : IServerActualizer
    {
        private readonly IApplicationConfig _config;
        private readonly IRouterClient _routerClient;
        private readonly IStatisticsProvider _statsProvider;
        private readonly ITaskScheduler _taskScheduler;
        
        private IPendingTask _actualizeTask;
        
        public RouterServerActualizer(IApplicationConfig config, IRouterClient routerClient, IStatisticsProvider statsProvider, ITaskScheduler taskScheduler)
        {
            _config = config;
            _routerClient = routerClient;
            _statsProvider = statsProvider;
            _taskScheduler = taskScheduler;
        }

        public async Task Actualize(int peersCount)
        {
            await _routerClient.Actualize(GetServerIdentity(), _config.GetServerName(), _config.GetRegion(), peersCount,
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
            return new ServerIdentity(_config.GetPublicName(),
                _config.GetListenPorts(), _config.GetServerRole());
        }
    }
}