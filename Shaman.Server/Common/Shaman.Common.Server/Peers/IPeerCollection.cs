using System.Collections.Concurrent;
using System.Net;
using Shaman.Common.Utils.Sockets;

namespace Shaman.Common.Server.Peers
{
    public interface IPeerCollection<T> 
        where T : class, IPeer, new()
    {
        void Add(IPEndPoint endPoint, IReliableSock socket);
        void Remove(IPEndPoint endPoint);
        void RemoveAll();
        T Get(IPEndPoint endPoint);
        ConcurrentDictionary<IPEndPoint, T> GetAll();
        int GetPeerCount();
    }
}