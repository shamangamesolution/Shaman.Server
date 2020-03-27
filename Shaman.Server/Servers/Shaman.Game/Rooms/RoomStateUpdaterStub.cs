using System;
using Shaman.Messages.MM;

namespace Shaman.Game.Rooms
{
    public class RoomStateUpdaterStub : IRoomStateUpdater
    {
        public void UpdateRoomState(Guid roomId, int roomPlayersCount, RoomState roomState, string matchMakerUrl)
        {
            //do nothing
        }
    }
}