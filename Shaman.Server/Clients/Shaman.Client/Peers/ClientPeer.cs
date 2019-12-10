using System;
using System.Collections.Generic;
using System.Net;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.LiteNetLibAdapter;

namespace Shaman.Client.Peers
{
    public class SimplePacketInfo : IPacketInfo
    {
        private readonly Action _releaseAction;

        public SimplePacketInfo(byte[] buffer, int offset, int length, Action releaseAction)
        {
            _releaseAction = releaseAction;
            Buffer = buffer;
            Offset = offset;
            Length = length;
        }

        public bool IsReliable { get; }
        public bool IsOrdered { get; }
        public byte[] Buffer { get; }
        public int Offset { get; }
        public int Length { get; }

        public void Dispose()
        {
            _releaseAction();
        }
    }

    public class ClientPacketSenderConfig : IPacketSenderConfig
    {
        private readonly int _maxPacketSize;
        private readonly int _sendTickTImerMs;

        public ClientPacketSenderConfig(int maxPacketSize, int sendTickTImerMs)
        {
            _maxPacketSize = maxPacketSize;
            _sendTickTImerMs = sendTickTImerMs;
        }

        public int GetMaxPacketSize()
        {
            return _maxPacketSize;
        }

        public int GetSendTickTimerMs()
        {
            return _sendTickTImerMs;
        }
    }

    class ServerSender : IPeerSender
    {
        private readonly IShamanLogger _logger;
        private readonly Action<DataPacket, Action> _onPackageReceived;
        private IReliableSock _socket;
        private PendingTask _socketTickTask = null;
        private IPEndPoint _ep;
        private readonly ITaskScheduler _taskScheduler;
        public Action<string> OnDisconnectedFromServer;

        private bool _connected;
        private readonly object _stateSync = new object();

        public ServerSender(IShamanLogger logger, Action<DataPacket, Action> onPackageReceived,
            ITaskScheduler taskScheduler)
        {
            _logger = logger;
            _onPackageReceived = onPackageReceived;
            _taskScheduler = taskScheduler;
            _connected = false;
        }

        private void OnDisconnected(IPEndPoint obj, string reason)
        {
            if (_ep.Equals(obj))
            {
                _connected = false;
                OnDisconnectedFromServer?.Invoke(reason);
            }
        }


        public void Connect(string address, int port)
        {
            //switch Sockets implementation.BEGIN
            //_socket = new HazelSock(_logger);
            _socket = new LiteNetSock(_logger);
            //switch sockets implementation.END

            _socket.OnPacketReceived += (endPoint, dataPacket, release) =>
            {
                _onPackageReceived(dataPacket, release);
                //todo if using hazel then this part should be considered in releaseAction param
//                _socket.ReturnBufferToPool(dataPacket.Buffer);
            };
            _socket.OnDisconnected += OnDisconnected;

            _socket.OnConnected += ep => { };

            _ep = GetIPEndPointFromHostName(address, port, false); // new IPEndPoint(IPAddress.Parse(address), port);
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
            }, 0, 10);
            _logger?.Debug($"Receive loop started");
        }

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

        public void Disconnect()
        {
            lock (_stateSync)
            {
                _connected = false;

                //stop ticking
                _taskScheduler.Remove(_socketTickTask);

                //_receiveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //Send(new DisconnectEvent());
                _socket?.Close();
            }
        }


        public bool IsConnected()
        {
            return _connected;
        }


        public void Send(PacketInfo packetInfo)
        {
            _socket.Send(packetInfo.Buffer, packetInfo.Offset, packetInfo.Length, packetInfo.IsReliable,
                packetInfo.IsOrdered);
        }
    }

    public class ClientPeer
    {
        private readonly IShamanLogger _logger;
        private readonly object _queueSync = new object();
        private readonly Queue<IPacketInfo> _packets = new Queue<IPacketInfo>();

        public Action OnPackageAvailable;

        private readonly ISerializer _serializer;
        private readonly PacketBatchSender _packetBatchSender;
        private readonly ServerSender _serverSender;

        public Action<string> OnDisconnectedFromServer
        {
            get => _serverSender.OnDisconnectedFromServer;
            set => _serverSender.OnDisconnectedFromServer = value;
        }

        public bool IsConnected()
        {
            return _serverSender.IsConnected();
        }

        public ClientPeer(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, int maxMessageSize,
            int sendTickMs)
        {
            _logger = logger;
            _serializer = new BinarySerializer();
            _packetBatchSender = new PacketBatchSender(taskSchedulerFactory,
                new ClientPacketSenderConfig(maxMessageSize, sendTickMs), _serializer);
            _serverSender = new ServerSender(logger, OnPackageReceived, taskSchedulerFactory.GetTaskScheduler());
        }

        private void OnPackageReceived(DataPacket packetInfo, Action release)
        {
            try
            {
                lock (_queueSync)
                {
                    _packets.Enqueue(new SimplePacketInfo(packetInfo.Buffer, packetInfo.Offset, packetInfo.Length,
                        release));
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

        public void Connect(string address, int port)
        {
            _serverSender.Connect(address, port);
            _packetBatchSender.Start(false);
        }

        public void Disconnect()
        {
            _serverSender.Disconnect();
            _packetBatchSender.Stop();
        }

        public IPacketInfo PopNextPacket()
        {
            lock (_queueSync)
            {
                if (_packets.Count > 0)
                    return _packets.Dequeue();
                else return null;
            }
        }

        public void Send(MessageBase message)
        {
            var initMsgArray = _serializer.Serialize(message);
            _packetBatchSender.AddPacket(_serverSender, initMsgArray, message.IsReliable, message.IsOrdered);
        }

        public int GetSendQueueLength()
        {
            return _packetBatchSender.GetMaxQueueSIze();
        }
    }
}