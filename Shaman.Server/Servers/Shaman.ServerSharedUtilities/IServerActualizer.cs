using System.Threading.Tasks;

namespace Shaman.ServerSharedUtilities
{
    public interface IServerActualizer
    {
        Task Actualize(int peersCount, ushort httpPort = 0, ushort httpsPort = 0);
    }
}