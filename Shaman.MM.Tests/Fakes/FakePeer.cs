using System;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;

namespace Shaman.MM.Tests.Fakes
{
    public class FakePeer : IPeer
    {
        private Guid _peerId = Guid.NewGuid();
        private Guid _sessionId = Guid.NewGuid();

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

        public void Initialize(IPEndPoint endpoint, Guid peerId, IReliableSock socket, ISerializer serializer,
            IApplicationConfig config, IShamanLogger logger)
        {
            throw new NotImplementedException();
        }

        public void Disconnect(DisconnectReason reason)
        {
            throw new NotImplementedException();
        }

        void IPeer.Send(PacketInfo packetInfo)
        {
            throw new NotImplementedException();
        }

        void IPeerSender.Send(PacketInfo packetInfo)
        {
            throw new NotImplementedException();
        }
    }
}