using System;
using System.Collections.Generic;
using Shaman.MM.Peers;
using Shaman.Messages.RoomFlow;

namespace Shaman.MM.MatchMaking
{
    
    public interface IMatchMaker
    {
        //init
        void Initialize(List<byte> requiredMatchMakingProperties);       
        
        //manage players list
        void AddPlayer(MmPeer peer, Dictionary<byte, object> properties);
        void RemovePlayer(Guid peerId);
        JoinInfo GetJoinInfo(Guid peerId);
        List<byte> GetRequiredProperties();
        
        //dispose
        void Clear();

        void AddMatchMakingGroup(int totalPlayersNeeded, int matchMakingTickMs, bool addBots, bool addOtherPlayers, int timeBeforeBotsAddedMs,
            Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures);

        void Start();
    }
}