using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Bundling.Common;
using Shaman.Contract.Routing;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Serialization.Messages;

namespace Shaman.Router.Data.Repositories
{
    public class StaticConfigurationRepository: IConfigurationRepository
    {
        private readonly EntityDictionary<ServerInfo> _serverInfos;
        private readonly EntityDictionary<BundleInfo> _bundleInfos;

        public class StaticRoutes
        {
            public List<ServerInfo> Servers { get; set; }
            public List<BundleInfo> Bundles { get; set; }
        }
        
        public StaticConfigurationRepository(StaticRoutes staticRoutes)
        {
            _serverInfos = new EntityDictionary<ServerInfo>(staticRoutes.Servers);
            _bundleInfos = new EntityDictionary<BundleInfo>(staticRoutes.Bundles);
        }

        public Task<EntityDictionary<ServerInfo>> GetAllServerInfo()
        {
            return Task.FromResult(_serverInfos);
        }

        public Task<List<int>> GetServerId(ServerIdentity identity)
        {
            return Task.FromResult<List<int>>(_serverInfos
                .Where(s => s.Address == identity.Address && s.Ports == identity.PortsString)
                .Select(s => s.Id)
                .ToList());
        }

        public Task<int> CreateServerInfo(ServerInfo serverInfo)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateServerInfoActualizedOn(int id, int peerCount, ushort httpPort, ushort httpsPort)
        {
            _serverInfos[id].ActualizedGap = TimeSpan.Zero;
            return Task.CompletedTask;
        }

        public Task<EntityDictionary<BundleInfo>> GetBundlesInfo()
        {
            return Task.FromResult(_bundleInfos);
        }
    }
}