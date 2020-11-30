using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Http;
using Shaman.Contract.Routing;
using Shaman.Contract.Routing.MM;
using Shaman.Messages.RoomFlow;
using Shaman.Serialization.Messages;

namespace Shaman.Tests.Providers
{
    public class FakeMatchMakerServerInfoProvider : IMatchMakerServerInfoProvider
    {
        private readonly IRequestSender _requestSender;
        private readonly string _gameServerAddress;
        private readonly string _gameServerPorts;
        EntityDictionary<ServerInfo> servers = new EntityDictionary<ServerInfo>();
        
        public FakeMatchMakerServerInfoProvider(IRequestSender requestSender, string gameServerAddress, string gameServerPorts)
        {
            _requestSender = requestSender;
            _gameServerAddress = gameServerAddress;
            _gameServerPorts = gameServerPorts;
            Start();
        }
        
        public void Start()
        {
            servers.Add(new ServerInfo(new ServerIdentity(_gameServerAddress,_gameServerPorts,ServerRole.GameServer), "", "", 7000, 0));
        }

        public void Stop()
        {
        }

        public EntityDictionary<ServerInfo> GetGameServers()
        {
            return servers;
        }

        public ServerInfo GetServer(int serverId)
        {
            if (!servers.ContainsKey(serverId))
                return null;

            return servers[serverId];
        }

        public async Task ActualizeMe()
        {
        }

        public ServerInfo GetLessLoadedServer()
        {
            return servers.FirstOrDefault();
        }

        public void AddServer(ServerInfo serverInfo)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> CreateRoom(int serverId, Guid roomId, Dictionary<byte, object> properties,
            Dictionary<Guid, Dictionary<byte, object>> players)
        {
            var response = await _requestSender.SendRequest<CreateRoomResponse>("", new CreateRoomRequest(properties, players));
            
            return response.RoomId;
        }

        public Task UpdateRoom(int serverId, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId)
        {
            return Task.CompletedTask;
        }

        public Task<string> GetBundleUri()
        {
            return Task.FromResult(string.Empty);
        }
    }
}