using System.Collections.Generic;
using Shaman.Common.Server.Messages;

namespace Shaman.Bundling.Balancing
{
    public interface IBundleInfoProviderConfig
    {
        string RouterUrl { get; set; }
        string PublicName { get; set; }
        IEnumerable<ushort> Ports { get; set; }
        ServerRole Role { get; set; }
    }
    
    public class BundleInfoProviderConfig : IBundleInfoProviderConfig
    {
        public string RouterUrl { get; set; }
        public string PublicName { get; set; }
        public IEnumerable<ushort> Ports { get; set; }
        public ServerRole Role { get; set; }

        public BundleInfoProviderConfig(string routerUrl, string publicName, IEnumerable<ushort> ports, ServerRole role)
        {
            RouterUrl = routerUrl;
            PublicName = publicName;
            Ports = ports;
            Role = role;
        }
    }
}