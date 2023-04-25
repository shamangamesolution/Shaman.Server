using System.Collections.Concurrent;
using System.Net;
using Shaman.Common.Udp.Sockets;

namespace Shaman.Common.Server.Peers
{
    public interface IPeerCollection<T> 
        where T : class, IPeer, new()
    {
        void Add(IPEndPoint endPoint, ITransportLayer socket);
        void Remove(IPEndPoint endPoint);
        void RemoveAll();
        T Get(IPEndPoint endPoint);
        ConcurrentDictionary<IPEndPoint, T> GetAll();
        int GetPeerCount();
        bool TryRemove(IPEndPoint endPoint, out T peer);
    }
}