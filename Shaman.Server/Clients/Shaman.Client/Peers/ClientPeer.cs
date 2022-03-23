using System;
using System.Collections.Generic;
using System.Net;
using Shaman.Common.Udp.Peers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

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


        public ClientPacketSenderConfig(int maxPacketSize, int sendTickTImerMs, int baseBufferSize = 64)
        {
            MaxPacketSize = maxPacketSize;
            SendTickTimeMs = sendTickTImerMs;
            BasePacketBufferSize = baseBufferSize;
        }

        public int MaxPacketSize { get; set; }
        public int BasePacketBufferSize { get; set; }
        public int SendTickTimeMs { get; set; }
    }

    class ServerSender : IPeerSender
    {
        private readonly IClientSocketFactory _clientSocketFactory;
        private readonly IShamanLogger _logger;
        private readonly Action<DataPacket, Action> _onPackageReceived;
        private IReliableSock _socket;
        private IPendingTask _socketTickTask = null;
        private IPEndPoint _ep;
        private readonly ITaskScheduler _taskScheduler;
        public Action<IDisconnectInfo> OnDisconnectedFromServer;
        public Action OnConnectedToServer;

        private bool _connected;
        private bool _isTicking = false;
        private object _isTickingMutex = new object();
        private readonly object _stateSync = new object();

        public ServerSender(IClientSocketFactory clientSocketFactory, IShamanLogger logger,
            Action<DataPacket, Action> onPackageReceived,
            ITaskScheduler taskScheduler)
        {
            _clientSocketFactory = clientSocketFactory;
            _logger = logger;
            _onPackageReceived = onPackageReceived;
            _taskScheduler = taskScheduler;
            _connected = false;
        }

        private void OnDisconnected(IPEndPoint obj, IDisconnectInfo info)
        {
            if (_ep.Equals(obj))
            {
                _connected = false;
                OnDisconnectedFromServer?.Invoke(info);
            }
        }

        public int GetRtt()
        {
            return _socket?.GetRtt() ?? 0;
        }

        public int GetPing()
        {
            return _socket?.GetPing() ?? 0;
        }

        public void Connect(string address, int port)
        {
            _socket = _clientSocketFactory.Create(_logger);

            _socket.OnPacketReceived += (endPoint, dataPacket, release) =>
            {
                _onPackageReceived(dataPacket, release);
            };
            _socket.OnDisconnected += OnDisconnected;

            _socket.OnConnected += ep => { OnConnectedToServer?.Invoke();};

            _ep = GetIPEndPointFromHostName(address, port, false); // new IPEndPoint(IPAddress.Parse(address), port);
            _socket.Connect(_ep);

            _connected = true;
            _isTicking = false;
            //Send(new ConnectedEvent());
            _socketTickTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                lock (_stateSync)
                {
                    if (!_connected)
                        return;

                    lock (_isTickingMutex)
                    {
                        if (_isTicking)
                            return;
                        _isTicking = true;
                    }

                    try
                    {
                        _socket.Tick();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Socket tick error: {ex}");
                    }
                    finally
                    {
                        lock(_isTickingMutex)
                            _isTicking = false;
                    }
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
        public void Disconnect(byte[] data, int offset, int length)
        {
            lock (_stateSync)
            {
                _connected = false;

                //stop ticking
                _taskScheduler.Remove(_socketTickTask);

                //_receiveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //Send(new DisconnectEvent());
                _socket?.Close(data, offset, length);
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

        public int Mtu => _socket.Mtu;
    }

    public class ClientPeer
    {
        private readonly IShamanLogger _logger;
        private readonly IClientSocketFactory _clientSocketFactory;
        private readonly object _queueSync = new object();
        private readonly Queue<IPacketInfo> _packets = new Queue<IPacketInfo>();

        public Action OnPackageAvailable;

        private readonly PacketBatchSender _packetBatchSender;
        private readonly ServerSender _serverSender;
        private readonly ShamanSender _shamanSender;

        public int Mtu => _serverSender.Mtu;

        public Action<IDisconnectInfo> OnDisconnectedFromServer
        {
            get => _serverSender.OnDisconnectedFromServer;
            set => _serverSender.OnDisconnectedFromServer = value;
        }
        public Action OnConnectedToServer
        {
            get => _serverSender.OnConnectedToServer;
            set => _serverSender.OnConnectedToServer = value;
        }

        public bool IsConnected()
        {
            return _serverSender.IsConnected();
        }

        public int GetRtt()
        {
            return _serverSender.GetRtt();
        }

        public int GetPing()
        {
            return _serverSender.GetPing();
        }

        public ClientPeer(IShamanLogger logger, IClientSocketFactory clientSocketFactory, ITaskSchedulerFactory taskSchedulerFactory, int maxMessageSize,
            int sendTickMs)
        {
            _logger = logger;
            _clientSocketFactory = clientSocketFactory;
            var clientPacketSenderConfig = new ClientPacketSenderConfig(maxMessageSize, sendTickMs);
            _packetBatchSender = new PacketBatchSender(taskSchedulerFactory, clientPacketSenderConfig, _logger);
            _serverSender = new ServerSender(clientSocketFactory, logger, OnPackageReceived, taskSchedulerFactory.GetTaskScheduler());
            _shamanSender = new ShamanSender(new BinarySerializer(), _packetBatchSender, clientPacketSenderConfig);
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
            lock (_queueSync)
            {
                _packets.Clear();
            }
        }
        public void Disconnect(byte[] data, int offset, int length)
        {
            _serverSender.Disconnect(data, offset, length);
            _packetBatchSender.Stop();
            lock (_queueSync)
            {
                _packets.Clear();
            }
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

        public int GetMessagesCountInQueue()
        {
            lock (_queueSync)
            {
                return _packets.Count;
            }
        }

        public int Send(ISerializable message, bool isReliable, bool isOrdered)
        {
            return _shamanSender.Send(message, new DeliveryOptions(isReliable, isOrdered), _serverSender);
        }

        public int GetSendQueueLength()
        {
            return _packetBatchSender.GetMaxQueueSIze();
        }
    }
}