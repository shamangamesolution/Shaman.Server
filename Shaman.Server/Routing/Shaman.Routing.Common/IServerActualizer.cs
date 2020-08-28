using System.Threading.Tasks;

namespace Shaman.Routing.Common.Actualization
{
    public interface IServerActualizer
    {
        Task Actualize(int peersCount);
        void Start(int actualizationPeriodMs);
        void Stop();
    }
}