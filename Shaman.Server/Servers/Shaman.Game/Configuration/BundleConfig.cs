using System.Collections.Generic;
using Shaman.Contract.Bundle;

namespace Shaman.Game.Configuration
{
    public class BundleConfig : IConfig
    {
        private readonly GameApplicationConfig _gameApplicationConfig;

        public BundleConfig(GameApplicationConfig gameApplicationConfig)
        {
            _gameApplicationConfig = gameApplicationConfig;
        }

        public string RouterUrl => _gameApplicationConfig.RouterUrl;
        public string HostAddress => _gameApplicationConfig.PublicDomainNameOrAddress;
        public IEnumerable<ushort> ListenPorts => _gameApplicationConfig.ListenPorts;
    }
}