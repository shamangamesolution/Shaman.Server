using System.Threading.Tasks;

namespace Shaman.Routing.Common
{
    public interface IServerActualizer
    {
        Task Actualize(int peersCount);
        void Start(int actualizationPeriodMs);
        void Stop();
    }
}