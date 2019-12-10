using System;
using System.Collections.Generic;
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
            Dictionary<Guid, Dictionary<byte, object>> bots, Dictionary<byte, object> roomProperties,
            Dictionary<byte, object> measures);
        
        JoinRoomResult JoinRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players);
        
        Room GetRoom(Guid groupId, int playersCount);
        IEnumerable<Room> GetRooms(Guid groupId, bool openOnly = true);
        int GetRoomsCount();
        void Start();
        void Stop();
    }
}