using System.Threading.Tasks;
using Shaman.Serialization.Messages;

namespace Shaman.Contract.Routing.Balancing
{
    public interface IRouterClient
    {
        Task<EntityDictionary<ServerInfo>> GetServerInfoList(bool actualOnly);
    }
}