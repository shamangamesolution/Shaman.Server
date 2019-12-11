using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Servers;
using Shaman.Messages;
using Shaman.Messages.General.Entity.Router;
using Shaman.MM.Providers;

namespace Shaman.MM.Tests.Fakes
{
    
    public class FakeServerProvider : IMatchMakerServerInfoProvider
    {
        private EntityDictionary<ServerInfo> _gameServers = new EntityDictionary<ServerInfo>();
        private bool _returnEmptyGuid = false;
        public FakeServerProvider(bool fillServers = true, bool returnEmptyGuid = false)
        {
            _returnEmptyGuid = returnEmptyGuid;

            if (fillServers)
            {
                _gameServers.Add(new ServerInfo(
                    new ServerIdentity("0.0.0.0", new List<ushort> {7777}, ServerRole.GameServer), "game1", "region1",
                    8000) {Id = 1});
            }
        }
        
        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public EntityDictionary<ServerInfo> GetGameServers()
        {
            return _gameServers;
        }

        public ServerInfo GetServer(int serverId)
        {
            return _gameServers[serverId];
        }

        public Task ActualizeMe()
        {
            throw new NotImplementedException();
        }

        public ServerInfo GetLessLoadedServer()
        {
            return _gameServers.FirstOrDefault();
        }

        public Guid CreateRoom(int serverId, Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            if (!_returnEmptyGuid)
                return Guid.NewGuid();
            else
                return Guid.Empty;
        }

        public void UpdateRoom(int serverId, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId)
        {
            
        }

        public Task<string> GetBundleUri()
        {
            throw new NotImplementedException();
        }
    }
}