using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization.Messages;
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
                        _gameServerList = BuildGameServersList();
                        
                        _logger.Info($"MatchMakerServerInfoProvider.GetServerInfoListResponse: i have {_gameServerList.Count()} game servers");
                        _isRequestingNow = false;
                    });


            }, 0, _config.ServerInfoListUpdateIntervalMs);
            
            
            _taskScheduler.ScheduleOnInterval(async () =>
            {
                //actualize
                //if (_gameServerList.Any())
                    await ActualizeMe();

            }, 0, _config.ActualizeMatchmakerIntervalMs);
            
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

        public ServerInfo GetLessLoadedServer()
        {
            return _gameServerList.OrderBy(s => s.PeerCount).FirstOrDefault();
        }

        private ServerInfo GetMe()
        {
            return _serverList.FirstOrDefault(s => s.Identity.Equals(_config.GetIdentity()) && s.ServerRole == ServerRole.MatchMaker);
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

            return _serverList.Where(s => s.Identity.Equals(_config.GetIdentity())).Select(s => s.Id).ToList();
        }

        private string GetUrl(int gameServerId)
        {
            var server = _gameServerList[gameServerId];
            if (server == null)
                throw new Exception($"GetUrl error: there is no game server with id = {gameServerId}");

            return UrlHelper.GetUrl(server.HttpPort, server.HttpsPort, server.Address);
        }
        
        public async Task<Guid> CreateRoom(int serverId, Guid roomId, Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            var url = GetUrl(serverId);
            var response = await _requestSender.SendRequest<CreateRoomResponse>(url, new CreateRoomRequest(properties, players)
            {
                RoomId = roomId
            });
            
            if (!response.Success)
            {
                var msg = $"CreateRoom error: {response.Message}";
                _logger.Error(msg);
                throw new Exception(msg);
            }
            
            return response.RoomId;
        }

        public async Task UpdateRoom(int serverId, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId)
        {
            var url = GetUrl(serverId);
            var response = await _requestSender.SendRequest<UpdateRoomResponse>(url, new UpdateRoomRequest(roomId, players));
            
            if (!response.Success)
            {
                var msg = $"UpdateRoom error: {response.Message}";
                _logger.Error(msg);
                throw new Exception(msg);
            }
        }
    }
}