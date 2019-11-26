using System;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Common.Server.Peers
{
    public abstract class PeerListenerBase<T>: IPeerListener<T> 
        where T : class, IPeer, new()

    {
        private IReliableSock _reliableSocket;
        private bool _isStopping = false;
        protected ISerializer Serializer;

        protected IPeerCollection<T> PeerCollection;
        
        protected IShamanLogger _logger;
        protected IApplicationConfig Config;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private PendingTask _socketTickTask;
        
        public abstract void OnReceivePacketFromClient(IPEndPoint endPoint, DataPacket dataPacket);

        private ushort _port;
        private ISocketFactory _socketFactory;
        protected IRequestSender RequestSender;

        
        private void OnReceivePacket(IPEndPoint endPoint, DataPacket data, Action release)
        {
            _taskScheduler.ScheduleOnceOnNow(() =>
            {
                try
                {
                    OnReceivePacketFromClient(endPoint,data);
                }
                finally
                {
                    release();
                    _reliableSocket.ReturnBufferToPool(data.Buffer);
                }
            });
        }       
        
        public virtual void Initialize(IShamanLogger logger, IPeerCollection<T> peerCollection, ISerializer serializer, IApplicationConfig config, ITaskSchedulerFactory taskSchedulerFactory, ushort port, ISocketFactory socketFactory, IRequestSender requestSender) 
        {
            _logger = logger;
            PeerCollection = peerCollection;
            Serializer = serializer;
            Config = config;
            _taskSchedulerFactory = taskSchedulerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _port = port;
            _socketFactory = socketFactory;
            RequestSender = requestSender;
        }

        public IPeerCollection<T> GetPeerCollection()
        {
            return PeerCollection;
        }        
        
        public void Listen()
        {
            //create reliable socket
            switch (Config.GetSocketType())
            {
                case SocketType.BareSocket:
                    _reliableSocket = _socketFactory.GetReliableSockWithBareSocket(_logger);
                    break;
                case SocketType.ThreadSocket:
                    _reliableSocket = _socketFactory.GetReliableSockWithThreadSocket(_logger);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _reliableSocket.Listen(_port);
            
            _reliableSocket.AddEventCallbacks(OnReceivePacket, OnNewClientConnect, OnClientDisconnect);

            _socketTickTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                if (_isStopping)
                    return;

                _reliableSocket.Tick();
            }, 0, Config.GetSocketTickTimeMs());
        }

        public ushort GetListenPort()
        {
            return _port;
        }

        public virtual void OnNewClientConnect(IPEndPoint endPoint)
        {
            _logger.Info($"Connected: {endPoint.Address} : {endPoint.Port}");
            //add peer to collection
            PeerCollection.Add(endPoint, _reliableSocket);
        }

        public virtual void OnClientDisconnect(IPEndPoint endPoint, string reason)
        {
            _logger.Info($"Disconnected: {endPoint.Address} : {endPoint.Port}. Reason: {reason}");
            PeerCollection.Remove(endPoint);
        }

        public void StopListening()
        {
            _isStopping = true;
            _taskScheduler.Remove(_socketTickTask);
            _taskScheduler.Dispose();
            _reliableSocket.Close();
        }
    }
}