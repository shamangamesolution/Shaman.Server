using Shaman.Common.Utils.Servers;

namespace Shaman.Common.Utils.Configuration
{
    public interface IApplicationCoreConfig
    {
        string GetRouterUrl();
        ServerIdentity GetIdentity();
    }
}