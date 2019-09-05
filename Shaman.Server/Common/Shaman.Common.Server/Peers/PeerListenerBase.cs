using System;
using System.Collections.Generic;
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
        private object _stateSync = new object();
        protected ISerializerFactory SerializerFactory;

        protected IPeerCollection<T> PeerCollection;
        
        protected IShamanLogger _logger;
        protected IApplicationConfig Config;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private PendingTask _socketTickTask, _receiveMessagesTask;
        
        private object _queueSync = new object();
        private Queue<PacketInfo> _packets = new Queue<PacketInfo>();
        
        public abstract void OnReceivePacketFromClient(PacketInfo obj);

        private ushort _port;
        private ISocketFactory _socketFactory;
        protected IRequestSender RequestSender;

        public PacketInfo GetNextPacket()
        {
            lock (_queueSync)
            {
                if (_packets.Count > 0)
                    return _packets.Dequeue();
                else
                    return null;

            }

        }
        
        private void OnReceivePacket(PacketInfo obj)
        {
            try
            {
                lock(_queueSync) 
                {
                    _packets.Enqueue(obj);
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error adding packet to queue: {ex}");
            }
        }       
        
        public virtual void Initialize(IShamanLogger logger, IPeerCollection<T> peerCollection, ISerializerFactory serializerFactory, IApplicationConfig config, ITaskSchedulerFactory taskSchedulerFactory, ushort port, ISocketFactory socketFactory, IRequestSender requestSender) 
        {
            _logger = logger;
            PeerCollection = peerCollection;
            SerializerFactory = serializerFactory;
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
                lock (_stateSync)
                {
                    if (_isStopping)
                        return;

                    _reliableSocket.Tick();
                }
            },  0, Config.GetSocketTickTimeMs());
            
            _receiveMessagesTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                lock (_stateSync)
                {
                    if (_isStopping)
                        return;
                }

                PacketInfo item = null;
                while((item = GetNextPacket()) != null)
                {
                    var item1 = item;
                    _taskScheduler.ScheduleOnceOnNow(() =>
                    {
                        OnReceivePacketFromClient(item1);
                        _reliableSocket.ReturnBufferToPool(item1.Buffer);
                    });
                } 

            }, 0, Config.GetReceiveTickTimerMs());
            
            
            
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
            lock (_stateSync)
            {
                _isStopping = true;
                _taskScheduler.Remove(_socketTickTask.Id);
                _taskScheduler.Remove(_receiveMessagesTask.Id);
                _reliableSocket.Close();
            }
        }

    }
}