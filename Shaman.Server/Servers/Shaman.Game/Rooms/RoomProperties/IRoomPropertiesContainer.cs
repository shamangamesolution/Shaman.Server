using System;
using System.Collections.Generic;

namespace Shaman.Game.Rooms.RoomProperties
{
    public interface IRoomPropertiesContainer
    {
        void Initialize(Dictionary<Guid, Dictionary<byte, object>> playersCameFromMatchMaker, Dictionary<byte, object> roomProperties);
        bool IsPlayerInMatchMakerCollection(Guid sessionId);
        
        void CheckIsBotForPlayers();
        int GetPlayersCount();
        int GetBotsNumber();
        bool IsRoomPropertiesContainsKey(byte key);
        T? GetRoomProperty<T>(byte key) where T : struct;
    }
}