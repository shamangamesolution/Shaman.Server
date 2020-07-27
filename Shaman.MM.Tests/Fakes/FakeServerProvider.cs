using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Utils.Serialization.Messages;
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

        public Task<Guid> CreateRoom(int serverId, Guid roomId, Dictionary<byte, object> properties,
            Dictionary<Guid, Dictionary<byte, object>> players)
        {
            if (!_returnEmptyGuid)
                return Task.FromResult(Guid.NewGuid());
            else
                return Task.FromResult(Guid.Empty);
        }

        public Task UpdateRoom(int serverId, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId)
        {
            return Task.CompletedTask;
        }

        public Task<string> GetBundleUri()
        {
            throw new NotImplementedException();
        }
    }
}