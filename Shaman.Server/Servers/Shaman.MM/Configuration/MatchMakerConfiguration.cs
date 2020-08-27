using System.Collections.Generic;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;

namespace Shaman.MM.Configuration
{
    public class MmApplicationConfig : ApplicationConfig
    {
        public int ServerUnregisterTimeoutMs { get; set; }
        public int ServerInfoListUpdateIntervalMs { get; set; }

        public void InitializeAdditionalParameters(int serverUnregisterTimeoutMs, int serverInfoListUpdateIntervalMs)
        {
            ServerUnregisterTimeoutMs = serverUnregisterTimeoutMs;
            ServerInfoListUpdateIntervalMs = serverInfoListUpdateIntervalMs;
        }
    }
}