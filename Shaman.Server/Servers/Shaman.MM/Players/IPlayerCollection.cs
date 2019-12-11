using System;
using System.Collections.Generic;
using Shaman.Messages.RoomFlow;

namespace Shaman.MM.Players
{
    public interface IPlayerCollection
    {
        void Add(MatchMakingPlayer player);
        IEnumerable<MatchMakingPlayer> GetPlayersAndSetOnMatchmaking(Guid peerId, int maxCount);
        //List<MatchMakingPlayer> GetPlayersByMeasureAndSetOnMatchMakingFlag(List<MatchMakingMeasure> measures, int totalNeeded);
        void Remove(Guid peerId);
        MatchMakingPlayer GetPlayer(Guid peerId);
        MatchMakingPlayer GetOldestPlayer();
        void Clear();
        int Count();
        void SetOnMatchmaking(Guid playerId, bool isOnMatchmaking);
        void AddMmGroup(Guid id, Dictionary<byte, object> properties);

    }
}