using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Servers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Configuration;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.Game.Providers
{
    public class GameServerInfoProvider : IGameServerInfoProvider
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly IStatisticsProvider _statsProvider;
        private ITaskScheduler _taskScheduler;
        private GameApplicationConfig _config;
    
        public GameServerInfoProvider(IRequestSender requestSender, ITaskSchedulerFactory taskSchedulerFactory, IApplicationConfig config, IShamanLogger logger, IStatisticsProvider statsProvider)
        {
            _requestSender = requestSender;
            _logger = logger;
            _statsProvider = statsProvider;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _config = (GameApplicationConfig) config;
        }
        
        public void Start()
        {
            _taskScheduler.ScheduleOnInterval(async () =>
            {
                //actualize
                ActualizeMe();
            }, 0, _config.ActualizationTimeoutMs);
            
        }

        public void Stop()
        {
            _taskScheduler.RemoveAll();
        }

        public async Task ActualizeMe()
        {
            _requestSender.SendRequest<ActualizeServerOnRouterResponse>(_config.GetRouterUrl(),
                new ActualizeServerOnRouterRequest(new ServerIdentity(_config.GetPublicName(),
                    _config.GetListenPorts(), _config.GetServerRole()), _config.GetServerName(), _config.GetRegion(), _statsProvider.GetPeerCount(), _config.BindToPortHttp),
                (response) =>
                {
                    if (!response.Success)
                    {
                        _logger.Error($"MatchMakerServerInfoProvider.ActualizeMe error: {response.Message}");
                    }
                });
        }
    }
}