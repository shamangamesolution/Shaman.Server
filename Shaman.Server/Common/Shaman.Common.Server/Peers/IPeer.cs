using System;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Udp.Peers;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

namespace Shaman.Common.Server.Peers
{
    public interface IPeer : IPeerSender
    {
        Guid GetPeerId();
        void SetSessionId(Guid sessionId);
        Guid GetSessionId();

        void Initialize(IPEndPoint endpoint, Guid peerId, ITransportLayer socket, ISerializer serializer,
            IApplicationConfig config, IShamanLogger logger);

        void Disconnect(ServerDisconnectReason reason);
    }
}