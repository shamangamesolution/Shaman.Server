using Shaman.Common.Server.Providers;
using Shaman.MM.Managers;

namespace Shaman.MM.Providers
{
    public class StatisticsProvider : IStatisticsProvider
    {
        private readonly IPlayersManager _playersManager;
        public StatisticsProvider(IPlayersManager playersManager)
        {
            _playersManager = playersManager;
        }

        public int GetPeerCount()
        {
            return _playersManager.Count();
        }
    }
}