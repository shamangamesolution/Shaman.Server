using System;
using System.Threading.Tasks;
using Shaman.Messages.MM;

namespace Shaman.Game.Rooms
{
    public interface IRoomStateUpdater
    {
        Task UpdateRoomState(Guid roomId, int roomPlayersCount, RoomState roomState, string matchMakerUrl);
    }
}