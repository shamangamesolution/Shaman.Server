using System.Net;
using Shaman.Common.Http;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Protection;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

namespace Shaman.Common.Server.Peers
{
    public interface IPeerListener<T> 
        where T : class, IPeer, new()
    {
        void Initialize(IShamanLogger logger, IPeerCollection<T> peerCollection, ISerializer serializer,
            IApplicationConfig config, ITaskSchedulerFactory taskSchedulerFactory, ushort port,
            IServerTransportLayerFactory serverTransportLayerFactory, IRequestSender requestSender,
            IProtectionManager banManager);
        void Listen();
        ushort GetListenPort();
        void StopListening();
        void OnReceivePacketFromClient(IPEndPoint endPoint, DataPacket dataPacket);
        bool OnNewClientConnect(IPEndPoint endPoint);
        void OnClientDisconnect(IPEndPoint endPoint, IDisconnectInfo info);
        int ResetTickDurationStatistics();
    }
}