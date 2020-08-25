using System.Linq;
using Shaman.Common.Server.Providers;
using Shaman.Game.Rooms;

namespace Shaman.Game.Providers
{
    public class StatisticsProvider : IStatisticsProvider
    {
        // private readonly IRoomManager _roomManager;
        public StatisticsProvider()
        {
            // _roomManager = roomManager;
        }

        public int GetPeerCount()
        {
            //return _roomManager.GetRoomPeerCount().Sum(r => r.Value);
            return -1;
        }
    }
}