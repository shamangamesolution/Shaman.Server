using System.Threading.Tasks;

namespace Shaman.Routing.Common.Actualization
{
    public class DefaultServerActualizer : IServerActualizer
    {
        public async Task Actualize(int peersCount)
        {
            //do nothing
        }

        public void Start(int actualizationPeriodMs)
        {
            //do nothing
        }

        public void Stop()
        {
            //do nothing
        }
    }
}