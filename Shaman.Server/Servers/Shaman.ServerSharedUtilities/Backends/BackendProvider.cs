using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.ServerSharedUtilities.Backends
{
    public class BackendProvider : IBackendProvider
    {
        private readonly IShamanLogger _logger;
        private readonly ITaskSchedulerFactory _taskSchedulerFactory;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IApplicationConfig _config;
        private readonly IRequestSender _requestSender;

        private List<ServerInfo> _backends = new List<ServerInfo>();
        private int _getBackendsListRequestCount = 0;
        private ServerInfo _me;
        
        public BackendProvider(ITaskSchedulerFactory taskSchedulerFactory, IApplicationConfig config, IRequestSender requestSender, IShamanLogger logger)
        {
            _taskSchedulerFactory = taskSchedulerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _config = config;
            _requestSender = requestSender;
            _logger = logger;
            Load().Wait();
        }

        public string GetFirstBackendUrl()
        {
            var backend = _backends.FirstOrDefault();
            if (backend == null)
                throw new Exception($"No backends in collection");

            return BuildBackendUrl(backend);
        }

        public string GetBackendUrl(int id)
        {
            var backend = _backends.FirstOrDefault(b => b.Id == id);
            if (backend == null)
                throw new Exception($"No backend with ID = {id}");
            return BuildBackendUrl(backend);
        }

        private static string BuildBackendUrl(ServerInfo backend)
        {
            var protocol = backend.HttpsPort > 0 ? "https" : "http";
            var port = backend.HttpPort == 0 ? backend.HttpsPort : backend.HttpPort;
            return $"{protocol}://{backend.Address}:{port}";
        }

        public void Start()
        {
            _taskScheduler.ScheduleOnInterval(async () => await Load(), 2000, _config.GetBackendListFromRouterIntervalMs());
        }

        private async Task Load()
        {
            var requestNumber = _getBackendsListRequestCount++;

            //request backends list
            var response = await _requestSender.SendRequest<GetServerInfoListResponse>(_config.GetRouterUrl(),
                    new GetServerInfoListRequest());

            if (response.ResultCode != ResultCode.OK)
            {
                _logger.Error($"BackendProvider error: error getting backends {response.ResultCode}|{response.Message}");
                return;
            }

            var meList =
                response.ServerInfoList.Where(s => s.Identity.Equals(_config.GetIdentity())).ToArray();
            if (!meList.Any())
            {
                _logger.Error($"BackendProvider.Load error: can not find me({_config.GetIdentity()}) in serve list");
                return;
            }

            if (meList.Length > 1)
            {
                _logger.Error($"BackendProvider.Load attention: more than 1 servers matched me - (record ids: {string.Join(",",meList.Select(m=>m.Id))}) in serve list");
            }

            _me = meList.First();

            var backends = response.ServerInfoList.Where(s =>
                s.ServerRole == ServerRole.BackEnd && s.Region == _me.Region &&
                s.IsApproved && s.ClientVersion == _me.ClientVersion).ToList();

            if (!backends.Any())
                _logger.Error($"Received 0 backends from Router!");
            else
            {
                if (requestNumber == 1)
                    _logger.Info($"Received {backends.Count} backends from Router");
                _backends = backends;
            }
        }
    }
}