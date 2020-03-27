using System;
using Shaman.Messages.MM;

namespace Shaman.Game.Rooms
{
    public interface IRoomStateUpdater
    {
        void UpdateRoomState(Guid roomId, int roomPlayersCount, RoomState roomState, string matchMakerUrl);
    }
}