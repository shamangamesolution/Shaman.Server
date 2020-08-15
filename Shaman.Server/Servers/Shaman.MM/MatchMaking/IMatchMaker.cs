using System;
using System.Collections.Generic;
using Shaman.Contract.MM;
using Shaman.MM.Peers;

namespace Shaman.MM.MatchMaking
{
    
    public interface IMatchMaker: IMatchMakingConfigurator
    {
        //init
        //manage players list
        void AddPlayer(MmPeer peer, Dictionary<byte, object> properties);
        void RemovePlayer(Guid peerId);
        List<byte> GetRequiredProperties();
        
        //dispose

        void Start();
        void Stop();
    }
}