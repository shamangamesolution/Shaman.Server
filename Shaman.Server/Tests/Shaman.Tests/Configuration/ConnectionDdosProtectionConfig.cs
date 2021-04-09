using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Protection;

namespace Shaman.Tests.Configuration
{
    public class ConnectionDdosProtectionConfig : IProtectionManagerConfig
    {
        public int MaxConnectsFromSingleIp { get; set; }
        public int ConnectionCountCheckIntervalMs { get; set; }
        public int BanCheckIntervalMs { get; set; }
        public int BanDurationMs { get; set; }
        public bool IsConnectionDdosProtectionOn { get; set; }

        public ConnectionDdosProtectionConfig(
            int maxConnectsFromSingleIp, 
            int checkIntervalMs, 
            int banCheckIntervalMs,
            int banMs)
        {
            MaxConnectsFromSingleIp = maxConnectsFromSingleIp;
            ConnectionCountCheckIntervalMs = checkIntervalMs;
            BanCheckIntervalMs = banCheckIntervalMs;
            BanDurationMs = banMs;
            IsConnectionDdosProtectionOn = true;
        }
    }
}