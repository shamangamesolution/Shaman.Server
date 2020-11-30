using System;
using System.Threading.Tasks;
using Shaman.Game.Rooms;
using Shaman.Messages.MM;

namespace Shaman.Launchers.Game.Standalone
{
    public class FakeRoomStateUpdater : IRoomStateUpdater
    {
        public async Task UpdateRoomState(Guid roomId, int roomPlayersCount, RoomState roomState, string matchMakerUrl,
            int maxMatchMakingWeight)
        {
            //do nothing
        }
    }
}