using Shaman.Common.Server.Providers;
using Shaman.MM.Players;

namespace Shaman.MM.Providers
{
    public class StatisticsProvider : IStatisticsProvider
    {
        private readonly IPlayerCollection _playerCollection;
        public StatisticsProvider(IPlayerCollection playerCollection)
        {
            _playerCollection = playerCollection;
        }

        public int GetPeerCount()
        {
            return _playerCollection.Count();
        }
    }
}