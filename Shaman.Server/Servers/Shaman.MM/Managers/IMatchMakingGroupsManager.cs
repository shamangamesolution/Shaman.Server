using System;
using System.Collections.Generic;

namespace Shaman.MM.Managers
{
    public interface IMatchMakingGroupsManager
    {
        Guid AddMatchMakingGroup(Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures);
        List<Guid> GetMatchmakingGroupIds(Dictionary<byte, object> playerProperties);
        void Start();
        void Stop();
    }
}