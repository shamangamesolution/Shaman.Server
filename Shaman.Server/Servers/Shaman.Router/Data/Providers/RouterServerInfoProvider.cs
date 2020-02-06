using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages;
using Shaman.Messages.General.Entity.Router;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories.Interfaces;

namespace Shaman.Router.Data.Providers
{
    public class RouterServerInfoProvider : IRouterServerInfoProvider
    {
        private readonly IConfigurationRepository _configRepo;
        private readonly ITaskSchedulerFactory _taskSchedulerFactory;
        private readonly IOptions<RouterConfiguration> _config;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IShamanLogger _logger;

        private bool _isRequestingNow = false;
        private EntityDictionary<ServerInfo> _serverList = new EntityDictionary<ServerInfo>();
        private EntityDictionary<BundleInfo> _bundlesList = new EntityDictionary<BundleInfo>();
        
        public RouterServerInfoProvider(IConfigurationRepository configRepo, ITaskSchedulerFactory taskSchedulerFactory, IOptions<RouterConfiguration> config, IShamanLogger logger)
        {
            _configRepo = configRepo;
            _taskSchedulerFactory = taskSchedulerFactory;
            _config = config;
            _logger = logger;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
        }
        
        public void Start()
        {
            // initial load
            LoadConfig().Wait();
            
            _taskScheduler.ScheduleOnInterval(async () =>
            {
                if (_isRequestingNow)
                    return;

                _isRequestingNow = true;

                await LoadConfig();

                _isRequestingNow = false;
                _logger.Info($"Received {_serverList.Count()} server info records");
                
            }, _config.Value.ServerInfoListUpdateIntervalMs, _config.Value.ServerInfoListUpdateIntervalMs);
            _logger.Info($"RouterServerInfoProvider started");
        }

        private async Task LoadConfig()
        {
            _serverList = await _configRepo.GetAllServerInfo();
            _bundlesList = await _configRepo.GetBundlesInfo();
        }

        public void Stop()
        {
            _taskScheduler.RemoveAll();
        }

        public EntityDictionary<ServerInfo> GetAllServers()
        {
            return _serverList;
        }
        
        public EntityDictionary<BundleInfo> GetAllBundles()
        {
            return _bundlesList;
        }
    }
}