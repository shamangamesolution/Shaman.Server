using System;
using System.Linq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.ServerSharedUtilities.Backends
{
    public class BackendProvider : IBackendProvider
    {
        private readonly IShamanLogger _logger;
        private readonly ITaskSchedulerFactory _taskSchedulerFactory;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IApplicationConfig _config;
        private readonly IRequestSender _requestSender;

        private ServerInfo[] _backends;
        private int _getBackendsListRequestCount = 0;
        
        public BackendProvider(ITaskSchedulerFactory taskSchedulerFactory, IApplicationConfig config, IRequestSender requestSender, IShamanLogger logger)
        {
            _taskSchedulerFactory = taskSchedulerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _config = config;
            _requestSender = requestSender;
            _logger = logger;
            Load();
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
            _taskScheduler.ScheduleOnInterval(Load, 0, _config.GetBackendListFromRouterIntervalMs());
        }

        private void Load()
        {
            var requestNumber = _getBackendsListRequestCount++;

            //request backends list
            var response =
                _requestSender.SendRequest<GetServerInfoListResponse>(_config.GetRouterUrl(),
                    new GetServerInfoListRequest()).Result;

            if (response.ResultCode != ResultCode.OK)
            {
                _logger.Error($"BackendProvider error: error getting backends {response.ResultCode}|{response.Message}");
                return;
            }

            var backends = response.ServerInfoList.Where(s=>s.ServerRole == ServerRole.BackEnd && s.IsApproved).ToArray();
            if (!backends.Any())
                _logger.Error($"Received 0 backends from Router!");
            else
            {
                if (requestNumber == 1)
                    _logger.Info($"Received {backends.Length} backends from Router");
                _backends = backends;
            }
        }
    }
}