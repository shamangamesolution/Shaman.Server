using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.HazelAdapter;

namespace Shaman.Client.Peers
{
    public class ClientPeer
    {
        private IReliableSock _socket;
        private IPEndPoint _ep;
        private bool _connected;
        private object _stateSync = new object();
        private readonly IShamanLogger _logger;
        private object _queueSync = new object();
        private Queue<PacketInfo> _packets = new Queue<PacketInfo>();
        private object _sendSync = new object();
        private ConcurrentQueue<PacketInfo> _packetsToSend = new ConcurrentQueue<PacketInfo>();

        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private PendingTask _socketTickTask = null;
        
        public Action OnPackageAvailable;
        public Action<string> OnDisconnectedFromServer;
        private ISerializerFactory _serializerFactory;
        private int _maxMessageSize = 0;
        
        private IPEndPoint GetIPEndPointFromHostName(string hostName, int port, bool throwIfMoreThanOneIP)
        {
            var addresses = System.Net.Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                throw new ArgumentException(
                    "Unable to retrieve address from specified host name.", 
                    "hostName"
                );
            }
            else if (throwIfMoreThanOneIP && addresses.Length > 1)
            {
                throw new ArgumentException(
                    "There is more that one IP address to the specified host.", 
                    "hostName"
                );
            }
            return new IPEndPoint(addresses[0], port); // Port gets validated here.
        }
        
        public bool IsConnected()
        {
            return _connected;
        }
        
        public ClientPeer(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, int maxMessageSize, int sendTickMs)
        {
            _logger = logger;
            _taskSchedulerFactory = taskSchedulerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _connected = false;
            _serializerFactory = new SerializerFactory(_logger);
            _maxMessageSize = maxMessageSize;
            _taskScheduler.ScheduleOnInterval(() => Send(), 0, sendTickMs);
        }

        private void OnDisconnected(IPEndPoint obj, string reason)
        {
            if (_ep.Equals(obj))
            {
                _connected = false;
                OnDisconnectedFromServer?.Invoke(reason);
            }
        }

        private void OnPackageReceived(PacketInfo packetInfo)
        {
            try
            {
                lock(_queueSync) 
                {
                    _packets.Enqueue(packetInfo);
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error adding packet to queue: {ex}");
            }

            //call event
            try
            {
                OnPackageAvailable?.Invoke();
            }
            catch (Exception ex)
            {
                _logger?.Error($"OnPackageAvailable Error: {ex}");

            }
        }
        
        public PacketInfo PopNextPacket()
        {
            lock (_queueSync)
            {
                if (_packets.Count > 0)
                    return _packets.Dequeue();
                else return null;

            }

        }
        
        public void Connect(string address, int port)
        {
            //switch Sockets implementation.BEGIN
            _socket = new HazelSock(_logger);
            //switch sockets implementation.END

            _socket.OnPacketReceived += packetInfo =>
            {
                OnPackageReceived(packetInfo);
                _socket.ReturnBufferToPool(packetInfo.Buffer);
            };
            _socket.OnDisconnected += OnDisconnected;

            _ep = GetIPEndPointFromHostName(address, port, false);// new IPEndPoint(IPAddress.Parse(address), port);
            _socket.Connect(_ep);
            _connected = true;
            //Send(new ConnectedEvent());
            _socketTickTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                lock (_stateSync)
                {
                    if (!_connected)
                        return;

                    _socket.Tick();
                }
            }, 0, 100);
            _logger?.Debug($"Receive loop started");
        }



        public void Disconnect()
        {
            lock (_stateSync)
            {
                _connected = false;
                
                //stop ticking
                if (_socketTickTask != null)
                    _taskScheduler.Remove(_socketTickTask.Id);
                
                //_receiveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //Send(new DisconnectEvent());
                _socket.Close();
            }
        }

        public void Send(MessageBase message)
        {
            var initMsgArray = message.Serialize(_serializerFactory);

            lock (_sendSync)
            {
                if (!_packetsToSend.IsEmpty)
                {
                    var lastElement = _packetsToSend.Last();
                    if (lastElement != null)
                    {
                        if (lastElement.Length + initMsgArray.Length <= _maxMessageSize
                            && lastElement.IsReliable == message.IsReliable
                            && lastElement.IsOrdered == message.IsOrdered)
                        {
                            //add to previous
                            lastElement.Add(initMsgArray, message.IsReliable, message.IsOrdered);
                            return;
                        }
                    }
                }

                var packetInfo = new PacketInfo(_maxMessageSize);
                packetInfo.Add(initMsgArray, message.IsReliable, message.IsOrdered);
                //add new packet
                _packetsToSend.Enqueue(packetInfo);
            }
        }
        
        private void Send()
        {
            lock (_sendSync)
            {
                while (!_packetsToSend.IsEmpty)
                {
                    if (!_packetsToSend.TryDequeue(out var pack))
                        continue;
                    _taskScheduler.ScheduleOnceOnNow(() =>
                        _socket.Send(pack.Buffer, 0, pack.Length, pack.IsReliable, pack.IsOrdered));
                }
            }
        }

        public int GetSendQueueLength()
        {
            lock (_sendSync)
            {
                return _packetsToSend.Count;
            }
        }

    }
}