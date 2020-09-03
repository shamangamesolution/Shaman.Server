using System.Threading.Tasks;
using Shaman.Contract.Routing.Actualization;

namespace Shaman.Launchers.Game.Standalone
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