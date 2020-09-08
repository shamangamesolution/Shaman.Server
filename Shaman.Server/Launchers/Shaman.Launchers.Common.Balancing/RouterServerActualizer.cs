using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.Actualization;
using Shaman.Routing.Balancing.Messages;

namespace Shaman.Launchers.Common.Balancing
{
    public class RouterServerActualizer : IServerActualizer
    {
        private readonly IRequestSender _requestSender;
        private readonly IStatisticsProvider _statsProvider;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IRoutingConfig _routingConfig;
        private readonly IShamanLogger _logger;

        private IPendingTask _actualizeTask;
        
        public RouterServerActualizer(IStatisticsProvider statsProvider, ITaskSchedulerFactory taskSchedulerFactory, IRequestSender requestSender,
            IRoutingConfig routingConfig, IShamanLogger logger)
        {
            _statsProvider = statsProvider;
            _requestSender = requestSender;
            _routingConfig = routingConfig;
            _logger = logger;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
        }

        public async Task Actualize(int peersCount)
        {
            var response = await _requestSender.SendRequest<ActualizeServerOnRouterResponse>(_routingConfig.RouterUrl,
                new ActualizeServerOnRouterRequest(_routingConfig.Identity, _routingConfig.ServerName, _routingConfig.Region, peersCount, _routingConfig.HttpPort, _routingConfig.HttpsPort));
            if (!response.Success)
            {
                _logger.Error($"MatchMakerServerInfoProvider.ActualizeMe error: {response.Message}");
            }
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
    }
}