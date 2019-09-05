using System;
using System.Collections.Concurrent;
using System.Net;
using Hazel;
using Hazel.Udp;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Sockets;

namespace Shaman.HazelAdapter
{
    public class HazelSock : IReliableSock
    {
        private UdpClientConnection ClientConnection;
        private UdpConnectionListener listener;
        private readonly ConcurrentDictionary<IPEndPoint, Connection> _endPointReceivers = new ConcurrentDictionary<IPEndPoint, Connection>();
        private IShamanLogger _logger;
        
        
        public HazelSock(IShamanLogger logger)
        {
            _logger = logger;
        }
        
        public void ReturnBufferToPool(byte[] buffer)
        {
            //nothing
        }
        
        public void AddEventCallbacks(Action<PacketInfo> onReceivePacket, Action<IPEndPoint> onConnect, Action<IPEndPoint, string> onDisconnect)
        {
            listener.NewConnection += args =>
            {
                _endPointReceivers.TryAdd(args.Connection.EndPoint, args.Connection);
                
                onConnect(args.Connection.EndPoint);
                
                args.Connection.Disconnected += (sender, eventArgs) =>
                {
                    _endPointReceivers.TryRemove(args.Connection.EndPoint, out var con);
                    onDisconnect(args.Connection.EndPoint, eventArgs.Reason);
                };

                args.Connection.DataReceived += eventArgs =>
                {
                    onReceivePacket(new PacketInfo(300)
                    {
                        EndPoint = args.Connection.EndPoint, 
                        Buffer = eventArgs.Message.Buffer,
                        Length = eventArgs.Message.Length,
                        Offset = eventArgs.Message.Offset,
                        RecycleCallback = () => eventArgs.Message.Recycle()
                    });
                    
                };
            };
        }

        public void Connect(IPEndPoint endPoint)
        {
            _logger?.Info($"Connect {endPoint.AddressFamily}|{endPoint.Address}:{endPoint.Port}");
            ClientConnection = new UdpClientConnection(endPoint);
            ClientConnection.DataReceived += args => OnPacketReceived?.Invoke(new PacketInfo(300)
            {
                EndPoint = endPoint, 
                Buffer = args.Message.Buffer,
                Length = args.Message.Length,
                Offset = args.Message.Offset,
            });
            ClientConnection.Disconnected += (sender, args) => { OnDisconnected?.Invoke(endPoint, args.Reason); }; 
            ClientConnection.Connect();
        }
        
        public void Listen(int port)
        {
            listener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, port));
            listener.Start();
        }

        public void Tick()
        {
            
        }

        public void Send(byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true)
        {
            ClientConnection.SendBytes(buffer, reliable ? SendOption.Reliable : SendOption.None);
        }

        public void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true)
        {
            if (_endPointReceivers.TryGetValue(endPoint, out var connection))
                connection.SendBytes(buffer, reliable ? SendOption.Reliable : SendOption.None);
        }

        public void Close()
        {
            listener?.Close();
            ClientConnection?.Dispose();
            _endPointReceivers.Clear();
        }

        public event Action<PacketInfo> OnPacketReceived;
        public event Action<IPEndPoint> OnConnected;
        public event Action<IPEndPoint> OnReliablePacketTimeout;
        public event Action<IPEndPoint, string> OnDisconnected;
    }
}