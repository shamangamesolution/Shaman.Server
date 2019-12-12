using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Servers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.Messages.General.Entity.Router;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Configuration;
using Shaman.ServerSharedUtilities;

namespace Shaman.MM.Providers
{
    public class MatchMakerServerInfoProvider : IMatchMakerServerInfoProvider
    {
        private readonly IRequestSender _requestSender;
        private readonly IShamanLogger _logger;
        private readonly IStatisticsProvider _statisticsProvider;
        private readonly IServerActualizer _serverActualizer;
        private readonly ITaskScheduler _taskScheduler;
        private readonly MmApplicationConfig _config;
        private bool _isRequestingNow;
        
        private EntityDictionary<ServerInfo> _gameServerList = new EntityDictionary<ServerInfo>();
        private EntityDictionary<ServerInfo> _serverList = new EntityDictionary<ServerInfo>();

        public MatchMakerServerInfoProvider(IRequestSender requestSender, ITaskSchedulerFactory taskSchedulerFactory,
            IApplicationConfig config, IShamanLogger logger, IStatisticsProvider statisticsProvider,
            IServerActualizer serverActualizer)
        {
            _requestSender = requestSender;
            _logger = logger;
            _statisticsProvider = statisticsProvider;
            _serverActualizer = serverActualizer;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _config = (MmApplicationConfig) config;
            _isRequestingNow = false;
            ActualizeMe().Wait();
        }
        
        public void Start()
        {
            _logger.Info($"Starting MatchMakerServerInfoProvider with period {_config.ServerInfoListUpdateIntervalMs} ms (actualizing once per {_config.ActualizeMatchmakerIntervalMs} ms)");

            _taskScheduler.ScheduleOnInterval(async () =>
            {
                if (_isRequestingNow)
                    return;

                _isRequestingNow = true;

                await _requestSender.SendRequest<GetServerInfoListResponse>(_config.GetRouterUrl(),
                    new GetServerInfoListRequest(), (response) =>
                    {
                        if (!response.Success)
                        {
                            _logger.Error($"MatchMakerServerInfoProvider.GetServerInfoListResponse: {response.Message}");
                            _isRequestingNow = false;
                            return;
                        }

                        _serverList = response.ServerInfoList;
                        
                        //fill game servers collection
                        FillGameServers();
                        
                        _logger.Info($"MatchMakerServerInfoProvider.GetServerInfoListResponse: i have {_gameServerList.Count()} game servers");
                        _isRequestingNow = false;
                    });


            }, 0, _config.ServerInfoListUpdateIntervalMs);
            
            
            _taskScheduler.ScheduleOnInterval(async () =>
            {
                //actualize
                //if (_gameServerList.Any())
                    await ActualizeMe();

            }, _config.ActualizeMatchmakerIntervalMs, _config.ActualizeMatchmakerIntervalMs);
            
        }

        public void Stop()
        {
            _taskScheduler.RemoveAll();
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

        public async Task ActualizeMe()
        {
            await _serverActualizer.Actualize(_statisticsProvider.GetPeerCount());
        }

        public async Task<string> GetBundleUri()
        {
            var response = await _requestSender.SendRequest<GetBundleUriResponse>(_config.GetRouterUrl(),
                new GetBundleUriRequest(GetServerIdentity()));
            
            if (!response.Success)
            {
                _logger.Error($"MatchMakerServerInfoProvider.GetBundleUri error: {response.Message}");
            }

            return response.BundleUri;
        }

        private ServerIdentity GetServerIdentity()
        {
            return new ServerIdentity(_config.GetPublicName(),
                _config.GetListenPorts(), _config.GetServerRole());
        }

        public ServerInfo GetLessLoadedServer()
        {
            return _gameServerList.OrderBy(s => s.PeerCount).FirstOrDefault();
        }

        private void FillGameServers()
        {
            _gameServerList = new EntityDictionary<ServerInfo>();
            var idList = GetIdList();
            var versions = new List<string>();
            foreach (var id in idList)
            {
                versions.Add(_serverList[id].ClientVersion);
            }

            foreach (var server in _serverList)
            {
                if (server.ServerRole == ServerRole.GameServer && versions.Contains(server.ClientVersion) && server.IsActual(_config.ServerUnregisterTimeoutMs))
                {
                    _gameServerList.Add(server);
                }
            }
        }

        private List<int> GetIdList()
        {
            return _serverList.Where(s => s.Identity.Equals(_config.GetIdentity())).Select(s => s.Id).ToList();
        }

        private string GetUrl(int gameServerId)
        {
            var server = _gameServerList[gameServerId];
            if (server == null)
                throw new Exception($"GetUrl error: there is no game server with id = {gameServerId}");

            var protocol = (server.HttpsPort > 0) ? "https" : "http";
            var port = (server.HttpsPort > 0) ? server.HttpsPort : server.HttpPort;
            
            return $"{protocol}://{server.Address}:{port}";
        }
        
        public Guid CreateRoom(int serverId, Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            var url = GetUrl(serverId);
            var response = _requestSender.SendRequest<CreateRoomResponse>(url, new CreateRoomRequest(properties, players)).Result;
            
            if (response == null || !response.Success)
            {
                _logger.Error($"CreateRoom error: response is null or error");
                //TODO bad kind of hack
                return Guid.Empty;
            }
            
            return response.RoomId;
        }

        public void UpdateRoom(int serverId, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId)
        {
            var url = GetUrl(serverId);
            var response = _requestSender.SendRequest<UpdateRoomResponse>(url, new UpdateRoomRequest(roomId, players)).Result;
            
            if (response == null || !response.Success)
            {
                _logger.Error($"CreateRoom error: response is null or error");
            }
        }
    }
}