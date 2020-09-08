using System.Collections.Generic;
using Shaman.Contract.Routing;

namespace Shaman.Launchers.Common.Balancing
{
    public interface IBalancingBundleInfoProviderConfig
    {
        string RouterUrl { get; set; }
        string PublicName { get; set; }
        IEnumerable<ushort> Ports { get; set; }
        ServerRole Role { get; set; }
    }
    
    public class BalancingBundleInfoProviderConfig : IBalancingBundleInfoProviderConfig
    {
        public string RouterUrl { get; set; }
        public string PublicName { get; set; }
        public IEnumerable<ushort> Ports { get; set; }
        public ServerRole Role { get; set; }

        public BalancingBundleInfoProviderConfig(string routerUrl, string publicName, IEnumerable<ushort> ports, ServerRole role)
        {
            RouterUrl = routerUrl;
            PublicName = publicName;
            Ports = ports;
            Role = role;
        }
    }
}