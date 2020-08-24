using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shaman.MM.Providers
{
    public interface IRoomApiProvider
    {
        Task<Guid> CreateRoom(string gameServerUrl, Guid roomId, Dictionary<byte, object> properties,
            Dictionary<Guid, Dictionary<byte, object>> players);
        
        Task UpdateRoom(string gameServerUrl, Dictionary<Guid, Dictionary<byte, object>> players, Guid roomId);
    }
}