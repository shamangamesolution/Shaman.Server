using System;
using System.Collections.Concurrent;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;

namespace Shaman.Common.Server.Peers
{
    public class PeerCollection<T> : IPeerCollection<T> 
        where T : class, IPeer, new()
    {
        private readonly ConcurrentDictionary<IPEndPoint, T> _peers = new ConcurrentDictionary<IPEndPoint, T>();
        
        private IShamanLogger _logger;
        private ISerializer _serializer;
        private IApplicationConfig _config;
        
        public PeerCollection(IShamanLogger logger, ISerializer serializer, IApplicationConfig config)
        {
            _logger = logger;
            _serializer = serializer;
            _config = config;
        }
        
        public void Add(IPEndPoint endPoint, IReliableSock socket)
        {
            var peer = new T();
            peer.Initialize(endPoint, Guid.NewGuid(), socket, _serializer, _config, _logger);            
            _peers.TryAdd(endPoint, peer);
        }


        public void Remove(IPEndPoint endPoint)
        {
            _peers.TryRemove(endPoint, out var peer);
        }
        
        public bool TryRemove(IPEndPoint endPoint, out T peer)
        {
            return _peers.TryRemove(endPoint, out peer);
        }

        public void RemoveAll()
        {
            _peers.Clear();
        }

        public T Get(IPEndPoint endPoint)
        {
            _peers.TryGetValue(endPoint, out var peer);
            return peer as T;
        }

        public ConcurrentDictionary<IPEndPoint, T> GetAll()
        {
            return _peers;
        }

        public int GetPeerCount()
        {
            return _peers.Count;
        }
    }
}