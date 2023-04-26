using System;
using System.Net;
using Shaman.Common.Http;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Protection;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

namespace Shaman.Common.Server.Peers
{
    public abstract class PeerListenerBase<T>: IPeerListener<T> 
        where T : class, IPeer, new()

    {
        private ITransportLayer _reliableSocket;
        private bool _isStopping = false;
        private bool _isTicking = false;
        private object _isTickingMutex = new object();
        protected ISerializer Serializer;

        protected IPeerCollection<T> PeerCollection;
        
        protected IShamanLogger _logger;
        protected IApplicationConfig Config;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        protected ITaskScheduler TaskScheduler;
        private IPendingTask _socketTickTask;
        private IProtectionManager _protectionManager;
        
        private int _maxSendDuration = int.MinValue;
        private DateTime _lastTick = DateTime.UtcNow;

        public int ResetTickDurationStatistics()
        {
            var duration = _maxSendDuration;
            _maxSendDuration = 0;
            return duration;
        }

        public abstract void OnReceivePacketFromClient(IPEndPoint endPoint, DataPacket dataPacket);

        private ListenPortDefinition _port;
        private IServerTransportLayerFactory _serverTransportLayerFactory;
        protected IRequestSender RequestSender;

        private void OnReceivePacket(IPEndPoint endPoint, DataPacket data, Action release)
        {
            if (_protectionManager.IsBanned(endPoint))
                return;
            
            try
            {
                OnReceivePacketFromClient(endPoint,data);
            }
            finally
            {
                //litenet automatically releases all null packets
                if (data.Buffer != null)
                    release();
            }
        }
        
        public virtual void Initialize(IShamanLogger logger, IPeerCollection<T> peerCollection, ISerializer serializer,
            IApplicationConfig config, ITaskSchedulerFactory taskSchedulerFactory, ListenPortDefinition port,
            IServerTransportLayerFactory serverTransportLayerFactory, IRequestSender requestSender,
            IProtectionManager protectionManager) 
        {
            _logger = logger;
            PeerCollection = peerCollection;
            Serializer = serializer;
            Config = config;
            _taskSchedulerFactory = taskSchedulerFactory;
            TaskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _port = port;
            _serverTransportLayerFactory = serverTransportLayerFactory;
            RequestSender = requestSender;
            _protectionManager = protectionManager;
        }

        public IPeerCollection<T> GetPeerCollection()
        {
            return PeerCollection;
        }        
        
        public void Listen()
        {
            _reliableSocket = _serverTransportLayerFactory.GetLayer(_port.Protocol);
            _logger.Info($"Network impl {_reliableSocket.GetType().Name} created");

            _reliableSocket.AddEventCallbacks(OnReceivePacket, OnNewClientConnect, OnClientDisconnect);
            _reliableSocket.Listen(_port.Port);
            
            _lastTick = DateTime.UtcNow;
            _isTicking = false;
            if (_reliableSocket.IsTickRequired)
                _socketTickTask = TaskScheduler.ScheduleOnInterval(() =>
                {
                    if (_isStopping)
                        return;
                    lock (_isTickingMutex)
                    {
                        if (_isTicking)
                            return;
                        _isTicking = true;
                    }

                    var duration = (DateTime.UtcNow - _lastTick).Milliseconds;
                    _lastTick = DateTime.UtcNow;
                    if (duration > _maxSendDuration) // overlapping not matters
                        _maxSendDuration = duration;

                    try
                    {
                        _reliableSocket.Tick();
                    }
                    catch
                    {
                        //empty
                    }
                    finally
                    {
                        lock (_isTickingMutex)
                            _isTicking = false;
                    }
                }, 0, Config.SocketTickTimeMs);
            
            //start protection
            _protectionManager.Start();
        }

        public ushort GetListenPort()
        {
            return _port.Port;
        }

        public virtual bool OnNewClientConnect(IPEndPoint endPoint)
        {
            _logger.Info($"Connected: {endPoint.Address} : {endPoint.Port}");
            try
            {
                if (_protectionManager.IsBanned(endPoint))
                    return false;
                _protectionManager.PeerConnected(endPoint);
                //add peer to collection
                PeerCollection.Add(endPoint, _reliableSocket);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"OnNewClientConnect error: {e}");
                return false;
            }
        }

        public void OnClientDisconnect(IPEndPoint endPoint, IDisconnectInfo info)
        {
            // _logger.Error($"Disconnected: {endPoint.Address} : {endPoint.Port}. Reason: {info.Reason}");
            if (!PeerCollection.TryRemove(endPoint, out var peer))
            {
                _logger.Warning(
                    $"OnClientDisconnect error: can not find peer for endpoint {endPoint.Address}:{endPoint.Port}");
                return;
            }

            PeerCollection.Remove(endPoint);
            ProcessDisconnectedPeer(peer, info);
        }

        protected abstract void ProcessDisconnectedPeer(T peer, IDisconnectInfo info);

        public void StopListening()
        {
            _isStopping = true;
            TaskScheduler.Remove(_socketTickTask);
            TaskScheduler.Dispose();
            _reliableSocket.Close();
            _protectionManager.Stop();
        }
    }
}
