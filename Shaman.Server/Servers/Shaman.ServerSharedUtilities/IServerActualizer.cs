using System.Threading.Tasks;

namespace Shaman.ServerSharedUtilities
{
    public interface IServerActualizer
    {
        Task Actualize(int peersCount);
    }
}