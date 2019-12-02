using System;
using System.Collections.Generic;
using Shaman.MM.Peers;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Contract;

namespace Shaman.MM.MatchMaking
{
    
    public interface IMatchMaker: IMatchMakingConfigurator
    {
        //init
        //manage players list
        void AddPlayer(MmPeer peer, Dictionary<byte, object> properties);
        void RemovePlayer(Guid peerId);
        JoinInfo GetJoinInfo(Guid peerId);
        List<byte> GetRequiredProperties();
        
        //dispose

        void Start();
        void Stop();
    }
}