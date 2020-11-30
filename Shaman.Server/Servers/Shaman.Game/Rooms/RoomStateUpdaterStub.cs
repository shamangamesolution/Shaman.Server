using System;
using System.Threading.Tasks;
using Shaman.Messages.MM;

namespace Shaman.Game.Rooms
{
    public class RoomStateUpdaterStub : IRoomStateUpdater
    {
        public async Task UpdateRoomState(Guid roomId, int roomPlayersCount, RoomState roomState, string matchMakerUrl,
            int maxMatchMakingWeight)
        {
            //do nothing
        }
    }
}