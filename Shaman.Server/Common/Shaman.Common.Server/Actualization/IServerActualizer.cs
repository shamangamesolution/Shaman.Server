using System.Threading.Tasks;

namespace Shaman.Common.Server.Actualization
{
    public interface IServerActualizer
    {
        Task Actualize(int peersCount);
        void Start(int actualizationPeriodMs);
        void Stop();
    }
}