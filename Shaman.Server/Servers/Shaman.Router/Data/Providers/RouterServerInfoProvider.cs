using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shaman.Common.Server.Messages;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Messages;
using Shaman.Serialization.Messages;

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
            var startNew = Stopwatch.StartNew();
            _serverList = await _configRepo.GetAllServerInfo();
            _bundlesList = await _configRepo.GetBundlesInfo();
            var updateElapsed = startNew.ElapsedMilliseconds;
            if (updateElapsed > 5000)
                _logger.Error($"Long data update: {updateElapsed}ms");

            var staleList = _serverList.Where(l => l.IsApproved && l.ServerRole != ServerRole.BackEnd && !l.IsActual(30000)).ToArray();
            if (staleList.Any())
            {
                _logger.Error($"Staled data ({string.Join(',',staleList.Select(s=>(DateTime.UtcNow - s.ActualizedOn.Value).TotalMilliseconds.ToString()))}) : {JsonConvert.SerializeObject(staleList, Formatting.Indented)}");
            }
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