using System.Collections.Generic;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Contract.Bundle;
using Shaman.Messages.General.Entity;

namespace Shaman.Game.Configuration
{
    public class GameApplicationConfig : ApplicationConfig
    {
        public bool OverwriteDownloadedBundle { get; set; }

        public GameApplicationConfig(string name,
            string regionName,
            string publicDomainNameOrIpAddress,
            List<ushort> ports,
            string routerUrl,
            ushort httpPort,
            bool isAuthOn = true,
            string authSecret = null,
            int socketTickTimeMs = 100,
            int receiveTickTimeMs = 33,
            int sendTickTimeMs = 50,
            SocketType socketType = SocketType.BareSocket,
            int actualizationIntervalMs = 1000)
            : base(name, regionName, ServerRole.GameServer, publicDomainNameOrIpAddress, ports, routerUrl, httpPort, socketType: socketType,
                 isAuthOn: isAuthOn,
                authSecret: authSecret, socketTickTimeMs: socketTickTimeMs, receiveTickTimeMs: receiveTickTimeMs,
                sendTickTimeMs: sendTickTimeMs, actualizationIntervalMs: actualizationIntervalMs)
        {
        }

        public IConfig GetBundleConfig()
        {
            return new BundleConfig(this);
        }
    }
}