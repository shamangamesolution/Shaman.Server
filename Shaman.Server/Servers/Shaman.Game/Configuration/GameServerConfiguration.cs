using System.Collections.Generic;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Contract.Bundle;
using Shaman.Messages.General.Entity;

namespace Shaman.Game.Configuration
{
    public class GameApplicationConfig : ApplicationConfig
    {
        public string MatchMakerUrl { get; set; }
        
        // public GameApplicationConfig(
        //     string region,
        //     string publicDomainNameOrIpAddress, 
        //     List<ushort> ports, 
        //     string routerUrl, 
        //     string name,
        //     ushort httpPort,
        //     int socketTickTimeMs, 
        //     int receiveTickTimeMs,
        //     int sendTickTimeMs,
        //     bool isAuthOn,
        //     string authSecret,
        //     SocketType socketType,
        //     int actualizationIntervalMs,
        //     bool overwriteDownloadedBundle,
        //     int maxPacketSize,
        //     int basePacketBufferSize)
        //     : base(name, region, ServerRole.MatchMaker, publicDomainNameOrIpAddress, ports, routerUrl, httpPort, socketTickTimeMs, receiveTickTimeMs, sendTickTimeMs,
        //         socketType, isAuthOn, authSecret, maxPacketSize, basePacketBufferSize, actualizationIntervalMs, overwriteDownloadedBundle)
        // {
        //
        // }

        public IConfig GetBundleConfig()
        {
            return new BundleConfig(this);
        }
        
        public void InitializeAdditionalParameters(string matchMakerUrl)
        {
            MatchMakerUrl = matchMakerUrl;
        }
    }
}