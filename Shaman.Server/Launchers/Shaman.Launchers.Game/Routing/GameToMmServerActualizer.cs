using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.Actualization;
using Shaman.Routing.Common.Messages;

namespace Shaman.Launchers.Game.Routing
{
    public class GameToMmServerActualizer : IServerActualizer
    {
        private readonly IMatchMakerInfoProvider _matchMakerInfoProvider;
        private readonly IApplicationConfig _config;
        private readonly IRequestSender _requestSender;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IStatisticsProvider _statsProvider;
        private readonly IShamanLogger _logger;
        private IPendingTask _actualizeTask;

        public GameToMmServerActualizer(IRequestSender requestSender, ITaskSchedulerFactory taskSchedulerFactory, IStatisticsProvider statsProvider, IApplicationConfig config, IShamanLogger logger, IMatchMakerInfoProvider matchMakerInfoProvider)
        {
            _requestSender = requestSender;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _statsProvider = statsProvider;
            _logger = logger;
            _matchMakerInfoProvider = matchMakerInfoProvider;
            _config = config;
        }

        public async Task Actualize(int peersCount)
        {
            var request = new ActualizeServerOnMatchMakerRequest(_config.GetIdentity(), _config.ServerName,
                _config.Region, _statsProvider.GetPeerCount(), _config.BindToPortHttp, 0);
            var response =
                await _requestSender.SendRequest<ActualizeServerOnMatchMakerResponse>(_matchMakerInfoProvider.MatchMakerUrl, request);
            if (!response.Success)
                _logger.Error($"GameToMmServerActualizer error: {response.Message}");
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