using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

namespace Shaman.Common.Server.Peers
{
    public interface IPeerListener<T> 
        where T : class, IPeer, new()
    {
        void Initialize(IShamanLogger logger, IPeerCollection<T> peerCollection, ISerializer serializer, IApplicationConfig config, ITaskSchedulerFactory taskSchedulerFactory, ushort port, ISocketFactory socketFactory, IRequestSender requestSender);
        void Listen();
        ushort GetListenPort();
        void StopListening();
        void OnReceivePacketFromClient(IPEndPoint endPoint, DataPacket dataPacket);
        void OnNewClientConnect(IPEndPoint endPoint);
        void OnClientDisconnect(IPEndPoint endPoint, IDisconnectInfo info);
        int ResetTickDurationStatistics();
    }
}