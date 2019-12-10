using System;
using System.Collections.Generic;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Players;

namespace Shaman.MM.Managers
{
    public interface IPlayersManager
    {
        void Add(MatchMakingPlayer player, List<Guid> groups);
        void Remove(Guid peerId);
        MatchMakingPlayer GetPlayer(Guid peerId);
        MatchMakingPlayer GetOldestPlayer();
        void SetOnMatchmaking(Guid playerId, bool isOnMatchmaking);
        void SetJoinInfo(Guid playerId, JoinInfo joinInfo, bool isMatchMakingComplete);
        void Clear();
        int Count();
        IEnumerable<MatchMakingPlayer> GetPlayers(Guid groupId, int maxCount);
//        void AddMmGroup(Guid id, Dictionary<byte, object> properties);
    }
}