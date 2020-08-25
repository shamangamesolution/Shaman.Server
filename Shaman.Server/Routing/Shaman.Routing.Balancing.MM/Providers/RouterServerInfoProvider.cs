using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Shaman.Common.Http;
using Shaman.Common.Server.Messages;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Routing.Balancing.Client;
using Shaman.Routing.Balancing.MM.Configuration;
using Shaman.Routing.Common.MM;
using Shaman.Serialization.Messages;

namespace Shaman.Routing.Balancing.MM.Providers
{
    public class MatchMakerServerInfoProvider : IMatchMakerServerInfoProvider
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly ITaskScheduler _taskScheduler;
        private IRouterClient _routerClient;
        private readonly IRouterServerInfoProviderConfig _config;
        private bool _isRequestingNow;
        private IPendingTask _getServerInfoTask;
        
        private EntityDictionary<ServerInfo> _gameServerList = new EntityDictionary<ServerInfo>();
        private EntityDictionary<ServerInfo> _serverList = new EntityDictionary<ServerInfo>();

        public MatchMakerServerInfoProvider(IRequestSender requestSender, ITaskSchedulerFactory taskSchedulerFactory,
            IRouterServerInfoProviderConfig config, IShamanLogger logger, IRouterClient routerClient)
        {
            _requestSender = requestSender;
            _logger = logger;
            _routerClient = routerClient;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _config = config;
            _isRequestingNow = false;
        }
        
        public void Start()
        {
            _getServerInfoTask = _taskScheduler.ScheduleOnInterval(async () =>
            {
                if (_isRequestingNow)
                    return;

                _isRequestingNow = true;
                _serverList = await _routerClient.GetServerInfoList();
                _gameServerList = BuildGameServersList();
                        
                _logger.Info($"MatchMakerServerInfoProvider.GetServerInfoListResponse: i have {_gameServerList.Count()} game servers");
                _isRequestingNow = false;
            }, 0, _config.ServerInfoListUpdateIntervalMs);
        }

        public void Stop()
        {
            _taskScheduler.Remove(_getServerInfoTask);
        }

        public EntityDictionary<ServerInfo> GetGameServers()
        {
            return _gameServerList;
        }

        public ServerInfo GetServer(int serverId)
        {
            if (!_serverList.ContainsKey(serverId))
                return null;

            return _serverList[serverId];
        }

        public ServerInfo GetLessLoadedServer()
        {
            return _gameServerList.OrderBy(s => s.PeerCount).FirstOrDefault();
        }

        private ServerInfo GetMe()
        {
            return _serverList.FirstOrDefault(s => s.Identity.Equals(_config.Identity) && s.ServerRole == ServerRole.MatchMaker);
        }
        
        private EntityDictionary<ServerInfo> BuildGameServersList()
        {
            var me = GetMe();
            if (me == null)
                throw new Exception($"There is no me in server list");
            
            var newGameServerList = new EntityDictionary<ServerInfo>();
            var idList = GetIdList();
            var versions = new List<string>();
            foreach (var id in idList)
            {
                versions.Add(_serverList[id].ClientVersion);
            }

            foreach (var server in _serverList)
            {
                if (server.ServerRole == ServerRole.GameServer && versions.Contains(server.ClientVersion) && server.Region == me.Region)
                {
                    if (server.IsActual(_config.ServerUnregisterTimeoutMs))
                        newGameServerList.Add(server);
                    else if(server.IsApproved)
                        _logger.Error($"Not actual server: {JsonConvert.SerializeObject(server)}");
                }
            }

            return newGameServerList;
        }

        private List<int> GetIdList()
        {

            return _serverList.Where(s => s.Identity.Equals(_config.Identity)).Select(s => s.Id).ToList();
        }


    }
}