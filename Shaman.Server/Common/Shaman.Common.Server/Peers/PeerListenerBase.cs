using System;
using System.Net;
using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Serialization;

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
        protected ITaskScheduler TaskScheduler;
        private IPendingTask _socketTickTask;

        private int _maxSendDuration = int.MinValue;
        private DateTime _lastTick = DateTime.UtcNow;

        public int ResetTickDurationStatistics()
        {
            var duration = _maxSendDuration;
            _maxSendDuration = 0;
            return duration;
        }

        public abstract void OnReceivePacketFromClient(IPEndPoint endPoint, DataPacket dataPacket);

        private ushort _port;
        private ISocketFactory _socketFactory;
        protected IRequestSender RequestSender;

        
        private void OnReceivePacket(IPEndPoint endPoint, DataPacket data, Action release)
        {
            TaskScheduler.ScheduleOnceOnNow(() =>
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
            TaskScheduler = _taskSchedulerFactory.GetTaskScheduler();
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
            
            _lastTick = DateTime.UtcNow;
            _socketTickTask = TaskScheduler.ScheduleOnInterval(() =>
            {
                if (_isStopping)
                    return;

                var duration = (DateTime.UtcNow - _lastTick).Milliseconds;
                _lastTick = DateTime.UtcNow;
                if (duration > _maxSendDuration) // overlapping not matters
                    _maxSendDuration = duration;

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

        public void OnClientDisconnect(IPEndPoint endPoint, IDisconnectInfo info)
        {
            using (info)
            {
                _logger.Info($"Disconnected: {endPoint.Address} : {endPoint.Port}. Reason: {info}");
                if (!PeerCollection.TryRemove(endPoint, out var peer))
                {
                    _logger.Warning($"OnClientDisconnect error: can not find peer for endpoint {endPoint.Address}:{endPoint.Port}");
                    return;
                }
                PeerCollection.Remove(endPoint);
                ProcessDisconnectedPeer(peer, info);
            }
        }

        protected abstract void ProcessDisconnectedPeer(T peer, IDisconnectInfo info);

        public void StopListening()
        {
            _isStopping = true;
            TaskScheduler.Remove(_socketTickTask);
            TaskScheduler.Dispose();
            _reliableSocket.Close();
        }
    }
}