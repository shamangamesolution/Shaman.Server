using System;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Sockets;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

namespace Shaman.Common.Server.Peers
{
    public interface IPeer : IPeerSender
    {
        Guid GetPeerId();
        void SetSessionId(Guid sessionId);
        Guid GetSessionId();

        void Initialize(IPEndPoint endpoint, Guid peerId, IReliableSock socket, ISerializer serializer,
            IApplicationConfig config, IShamanLogger logger);

        void Disconnect(ServerDisconnectReason reason);
    }
}