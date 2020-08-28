using System.Threading.Tasks;
using Shaman.Contract.Routing.Actualization;
using Shaman.Routing.Common;

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