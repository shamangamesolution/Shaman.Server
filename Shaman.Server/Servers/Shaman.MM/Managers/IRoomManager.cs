using System;
using System.Collections.Generic;
using Shaman.Common.Server.Peers;
using Shaman.Messages.MM;
using Shaman.MM.Rooms;

namespace Shaman.MM.Managers
{
    public enum RoomOperationResult
    {
        OK,
        ServerNotFound = 2,
        CreateRoomError = 3,
        JoinRoomError = 4
    }

    public class JoinRoomResult
    {
        public RoomOperationResult Result;
        public string Address;
        public ushort Port;
        public Guid RoomId;
    }
    
    public interface IRoomManager
    {
        JoinRoomResult CreateRoom(Guid groupId, Dictionary<Guid, Dictionary<byte, object>> players,
            Dictionary<byte, object> roomProperties);
        JoinRoomResult JoinRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players);
        void UpdateRoomState(Guid roomId, int currentPlayers, RoomState roomState);
        Room GetRoom(Guid groupId, int playersCount);
        Room GetRoom(Guid roomId);
        IEnumerable<Room> GetAllRooms();
        IEnumerable<Room> GetRooms(Guid groupId, bool openOnly = true, int limit = 10);
        int GetRoomsCount();
        void Start(int timeToKeepCreatedRoomSec);
        void Stop();
    }
}