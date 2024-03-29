using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<JoinRoomResult> CreateRoom(Guid groupId, Dictionary<Guid, Dictionary<byte, object>> players,
            Dictionary<byte, object> roomProperties);
        Task<JoinRoomResult> JoinRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players, int maxWeightInList, int totalWeightInList);
        void UpdateRoomState(Guid roomId, int currentPlayers, RoomState roomState, int maxWeightToJoin);
        Room GetRoom(Guid groupId, int playersCount, int maxWeightInPlayersList, int totalWeightOnPlayerList);
        Room GetRoom(Guid roomId);
        IEnumerable<Room> GetAllRooms();
        IEnumerable<Room> GetRooms(Guid groupId, bool openOnly = true, int limit = 10);
        int GetRoomsCount();
        void Start(int timeToKeepCreatedRoomSec, int timeIntervalToCloseOpenRoomsWithoutUpdatesMs = 5000);
        void Stop();
    }
}