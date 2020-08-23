using System.Threading.Tasks;

namespace Shaman.Common.Server.Actualization
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