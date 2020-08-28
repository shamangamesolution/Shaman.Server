using System.Threading.Tasks;
using Shaman.Common.Server.Messages;
using Shaman.Serialization.Messages;

namespace Shaman.Routing.Balancing.Contracts
{
    public interface IRouterClient
    {
        Task<EntityDictionary<ServerInfo>> GetServerInfoList(bool actualOnly);
    }
}