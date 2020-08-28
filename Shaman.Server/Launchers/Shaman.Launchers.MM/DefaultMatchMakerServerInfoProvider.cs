using System.Linq;
using Shaman.Common.Server.Messages;
using Shaman.Contract.Common.Logging;
using Shaman.Routing.Common.MM;
using Shaman.Serialization.Messages;

namespace Shaman.Launchers.MM
{
    public class DefaultMatchMakerServerInfoProvider : IMatchMakerServerInfoProvider
    {
        private readonly IShamanLogger _logger; 
        
        private object _mutex = new object();
        private EntityDictionary<ServerInfo> _gameServerList = new EntityDictionary<ServerInfo>();
        private int _index = 1;

        public DefaultMatchMakerServerInfoProvider(IShamanLogger logger)
        {
            _logger = logger;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        private int GetIndex()
        {
            lock (_mutex)
            {
                return _index++;
            }
        }

        public EntityDictionary<ServerInfo> GetGameServers()
        {
            lock (_mutex)
            {
                return _gameServerList;
            }
        }

        public ServerInfo GetServer(int serverId)
        {
            lock (_mutex)
            {
                return _gameServerList.FirstOrDefault(s => s.Id == serverId);
            }
        }

        public ServerInfo GetLessLoadedServer()
        {
            lock (_mutex)
            {
                return _gameServerList.OrderBy(s => s.PeerCount).FirstOrDefault();
            }
        }

        public void AddServer(ServerInfo serverInfo)
        {
            lock (_mutex)
            {
                serverInfo.Id = GetIndex();
                _gameServerList.Add(serverInfo);
                _logger.Error($"Registered game server: {serverInfo.Identity}");
            }
        }
    }
}