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
        
        public void InitializeAdditionalParameters(string matchMakerUrl)
        {
            MatchMakerUrl = matchMakerUrl;
        }
    }
}