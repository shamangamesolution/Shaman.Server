using System;
using System.Collections.Generic;

namespace Shaman.Contract.Bundle
{
    public interface IRoomPropertiesContainer
    {
        void Initialize(Dictionary<Guid, Dictionary<byte, object>> playersCameFromMatchMaker, Dictionary<byte, object> roomProperties);
        void AddNewPlayers(Dictionary<Guid, Dictionary<byte, object>> playersCameFromMatchMaker);
        bool IsPlayerInMatchMakerCollection(Guid sessionId);
        
        void CheckIsBotForPlayers();
        int GetPlayersCount();
        int GetBotsNumber();
        bool IsRoomPropertiesContainsKey(byte key);
        T? GetRoomProperty<T>(byte key) where T : struct;
        string GetRoomPropertyAsString(byte key);
        int GetPlayerCountToStartGame();
        void RemovePlayer(Guid sessionId);
        Dictionary<byte, object> GetRoomProperties();
    }
}