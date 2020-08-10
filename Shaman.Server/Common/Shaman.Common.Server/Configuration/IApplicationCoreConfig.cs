using Shaman.Router.Messages;

namespace Shaman.Common.Server.Configuration
{
    public interface IApplicationCoreConfig
    {
        string GetRouterUrl();
        ServerIdentity GetIdentity();
    }
}