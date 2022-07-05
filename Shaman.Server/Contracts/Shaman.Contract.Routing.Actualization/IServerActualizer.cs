using System.Threading.Tasks;

namespace Shaman.Contract.Routing.Actualization
{
    public interface IServerActualizer
    {
        Task Actualize(int peersCount, string state = null);
        void Start(int actualizationPeriodMs);
        void Stop();
    }
}