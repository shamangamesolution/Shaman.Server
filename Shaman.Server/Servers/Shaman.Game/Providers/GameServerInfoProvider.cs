using System.Threading.Tasks;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Game.Configuration;
using Shaman.ServerSharedUtilities;

namespace Shaman.Game.Providers
{
    public class GameServerInfoProvider : IGameServerInfoProvider
    {
        private readonly IStatisticsProvider _statsProvider;
        private readonly IServerActualizer _serverActualizer;
        private readonly ITaskScheduler _taskScheduler;
        private readonly GameApplicationConfig _config;

        public GameServerInfoProvider(ITaskSchedulerFactory taskSchedulerFactory,
            IApplicationConfig config, IStatisticsProvider statsProvider,
            IServerActualizer serverActualizer)
        {
            _statsProvider = statsProvider;
            _serverActualizer = serverActualizer;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _config = (GameApplicationConfig) config;
        }
        
        public void Start()
        {
            _taskScheduler.ScheduleOnInterval(async () =>
            {
                //actualize
                await ActualizeMe();
            }, 0, _config.ActualizationTimeoutMs);
            
        }

        public void Stop()
        {
            _taskScheduler.RemoveAll();
        }

        public async Task ActualizeMe()
        {
            await _serverActualizer.Actualize(_statsProvider.GetPeerCount());
        }

        public string GetMatchMakerWebUrl(int matchMakerId)
        {
            throw new System.NotImplementedException();
        }
    }
}