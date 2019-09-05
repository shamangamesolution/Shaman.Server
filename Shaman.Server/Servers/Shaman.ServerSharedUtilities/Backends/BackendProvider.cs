using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.ServerSharedUtilities.Backends
{
    public class BackendProvider : IBackendProvider
    {
        private IShamanLogger _logger;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private IApplicationConfig _config;
        private IRequestSender _requestSender;

        private object _syncDict = new object();
        private List<Backend> _backends;
        private int _getBackendsListRequestCount = 0;
        
        public BackendProvider(ITaskSchedulerFactory taskSchedulerFactory, IApplicationConfig config, IRequestSender requestSender, IShamanLogger logger)
        {
            _taskSchedulerFactory = taskSchedulerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _config = config;
            _requestSender = requestSender;
            _logger = logger;
            _backends = new List<Backend>();
        }

        public string GetFirstBackendUrl()
        {
            lock (_syncDict)
            {
                var backend = _backends.FirstOrDefault();
                if (backend == null)
                    throw new Exception($"No backends in collection");

                
                
                return $"{backend.Address}:{backend.Port}";
            } 
        }

        public string GetBackendUrl(int id)
        {
            lock (_syncDict)
            {
                var backend = _backends.FirstOrDefault(b => b.Id == id);
                if (backend == null)
                    throw new Exception($"No backend with ID = {id}");

                
                
                return $"{backend.Address}:{backend.Port}";
            }
        }

        public void Start()
        {
            _taskScheduler.ScheduleOnInterval(() =>
            {
                _getBackendsListRequestCount++;
                
                //request backends list
                var response =
                    _requestSender.SendRequest<GetBackendsListResponse>(_config.GetRouterUrl(),
                        new GetBackendsListRequest()).Result;

                if (response.ResultCode != ResultCode.OK)
                {
                    _logger.Error($"BackendProvider error: error getting backends {response.ResultCode}|{response.Message}");
                    return;
                }
                
                if (response.Backends.Count == 0)
                    _logger.Error($"Received 0 backends from Router!");
                else
                {
                    if (_getBackendsListRequestCount == 1)
                        _logger.Info($"Received {response.Backends.Count} backends from Router");
                    lock (_syncDict)
                    {
                        _backends = response.Backends;
                    }
                }
            }, 0, _config.GetBackendListFromRouterIntervalMs());
        }
    }
}