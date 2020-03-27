using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.MM.Players;
using Shaman.MM.Rooms;

namespace Shaman.MM.Managers
{
    public interface IMatchMakingGroupsManager
    {
        //Guid AddMatchMakingGroup(Dictionary<byte, object> measures);
        //List<Guid> GetMatchmakingGroupIds(Dictionary<byte, object> playerProperties);
        //Dictionary<byte, object> GetRoomProperties(Guid groupId);
        Guid AddMatchMakingGroup(Dictionary<byte, object> measures);
        IEnumerable<Room> GetRooms(Dictionary<byte, object> playerProperties);
        void AddPlayerToMatchMaking(MatchMakingPlayer player);
        Task<JoinRoomResult> CreateRoom(Guid sessionId, Dictionary<byte, object> playerProperties);
        void Start(int timeToKeepCreatedRoomSec = 1800, int timeIntervalToCloseOpenRoomsWithoutUpdatesMs = 5000);
        void Stop();
    }
}