using System;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

namespace Shaman.MM.Tests.Fakes
{
    public class FakePeer : IPeer
    {
        private readonly Guid _peerId = Guid.NewGuid();
        private readonly Guid _sessionId = Guid.NewGuid();

        public FakePeer()
        {
            
        }
        
        public Guid GetPeerId()
        {
            return _peerId;
        }

        public void SetSessionId(Guid sessionId)
        {
            throw new NotImplementedException();
        }

        public Guid GetSessionId()
        {
            return _sessionId;
        }

        public void Initialize(IPEndPoint endpoint, Guid peerId, ITransportLayer socket, ISerializer serializer,
            IApplicationConfig config, IShamanLogger logger)
        {
            throw new NotImplementedException();
        }

        public void Disconnect(ServerDisconnectReason reason)
        {
            throw new NotImplementedException();
        }

        public void Send(PacketInfo packetInfo)
        {
            throw new NotImplementedException();
        }

        public int Mtu => 0;
    }
}