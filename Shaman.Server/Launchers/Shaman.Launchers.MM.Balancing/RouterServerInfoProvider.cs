using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Contract.Routing.Balancing;
using Shaman.Contract.Routing.MM;
using Shaman.Serialization.Messages;

namespace Shaman.Launchers.MM.Balancing
{
    public class MatchMakerServerInfoProvider : IMatchMakerServerInfoProvider
    {
        private readonly IShamanLogger _logger;
        private readonly ITaskScheduler _taskScheduler;
        private IRouterClient _routerClient;
        private readonly IRouterServerInfoProviderConfig _config;
        private bool _isRequestingNow;
        private IPendingTask _getServerInfoTask;
        
        private EntityDictionary<ServerInfo> _gameServerList = new EntityDictionary<ServerInfo>();
        private EntityDictionary<ServerInfo> _serverList = new EntityDictionary<ServerInfo>();

        public MatchMakerServerInfoProvider(ITaskSchedulerFactory taskSchedulerFactory,
            IRouterServerInfoProviderConfig config, IShamanLogger logger, IRouterClient routerClient)
        {
            _logger = logger;
            _routerClient = routerClient;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _config = config;
            _isRequestingNow = false;
        }

        private async Task GetList()
        {
            if (_isRequestingNow)
                return;

            _isRequestingNow = true;
            try
            {
                _serverList = await _routerClient.GetServerInfoList(false);
                _gameServerList = BuildGameServersList();
                _logger.Info($"MatchMakerServerInfoProvider.GetServerInfoListResponse: i have {_gameServerList.Count()} game servers");
            }
            finally
            {
                _isRequestingNow = false;
            }
        }
        
        public void Start()
        {
            _getServerInfoTask = _taskScheduler.ScheduleOnInterval(async () => await GetList(), 0, _config.ServerInfoListUpdateIntervalMs);
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

        public void AddServer(ServerInfo serverInfo)
        {
            throw new Exception($"This operation is not allowed on this type of launch");
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
            var serverList = GetMyMmList().ToList();
            // var versions = new List<string>();
            // foreach (var id in idList)
            // {
            //     versions.Add(_serverList[id].ClientVersion);
            // }

            foreach (var server in _serverList)
            {
                // if (server.ServerRole == ServerRole.GameServer && versions.Contains(server.ClientVersion) && server.Region == me.Region)
                if (server.ServerRole == ServerRole.GameServer && serverList.Any(s => s.AreVersionsIntersect(server)) && server.Region == me.Region)
                {
                    if (server.IsActual(_config.ServerUnregisterTimeoutMs))
                        newGameServerList.Add(server);
                    else if(server.IsApproved)
                        _logger.Error($"Not actual server: {JsonConvert.SerializeObject(server)}");
                }
            }

            return newGameServerList;
        }

        private IEnumerable<ServerInfo> GetMyMmList()
        {

            return _serverList.Where(s => s.Identity.Equals(_config.Identity));
        }


    }
}