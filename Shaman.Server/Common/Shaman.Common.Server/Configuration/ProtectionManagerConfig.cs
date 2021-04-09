namespace Shaman.Common.Server.Configuration
{
    public interface IProtectionManagerConfig
    {
        int MaxConnectsFromSingleIp { get; set; }
        int ConnectionCountCheckIntervalMs { get; set; }
        int BanCheckIntervalMs { get; set; }
        int BanDurationMs { get; set; }
        bool IsConnectionDdosProtectionOn { get; set; }
    }
}