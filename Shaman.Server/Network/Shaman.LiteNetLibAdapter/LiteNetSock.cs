using System;
using System.Collections.Concurrent;
using System.Net;
using LiteNetLib;
using Shaman.Common.Contract;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Sockets;
using DisconnectReason = LiteNetLib.DisconnectReason;

namespace Shaman.LiteNetLibAdapter
{
    public class LiteNetSock : IReliableSock
    {
        private readonly EventBasedNetListener _listener;
        private readonly NetManager _peer;
        private NetPeer _serverPeer;
        private readonly IShamanLogger _logger;
        private readonly ConcurrentDictionary<IPEndPoint, NetPeer> _endPointReceivers = new ConcurrentDictionary<IPEndPoint, NetPeer>();

        public LiteNetSock(IShamanLogger logger)
        {
            _logger = logger;
            _listener = new EventBasedNetListener();
            _peer = new NetManager(_listener);
            _peer.ReconnectDelay = 100;
            _peer.MaxConnectAttempts = 50;
        }
        
        public void Connect(IPEndPoint endPoint)
        {
            _peer.Start();
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var dataPacket = new DataPacket(dataReader.RawData, dataReader.UserDataOffset, dataReader.UserDataSize,
                    ConvertDeliveryMethod(deliveryMethod));
                OnPacketReceived?.Invoke(endPoint, dataPacket, dataReader.Recycle);
            };
            _listener.PeerConnectedEvent += peer =>
            {
                _serverPeer = peer;
                OnConnected?.Invoke(peer.EndPoint);
            };
            
            _listener.PeerDisconnectedEvent += (peer, info) =>
            {
                _serverPeer = null;
                OnDisconnected?.Invoke(endPoint, BuildDisconnectInfo(info));
            };
            _peer.Connect(endPoint.Address.ToString(), endPoint.Port, "SomeConnectionKey333");
        }

        private static IDisconnectInfo BuildDisconnectInfo(DisconnectInfo info)
        {
            return new LightNetDisconnectInfo(info.Reason,
                info.Reason == DisconnectReason.RemoteConnectionClose ? info.AdditionalData : null);
        }

        public void AddEventCallbacks(Action<IPEndPoint, DataPacket, Action> onReceivePacket, Action<IPEndPoint> onConnect, Action<IPEndPoint, IDisconnectInfo> onDisconnect)
        {
            _listener.ConnectionRequestEvent += request => { request.AcceptIfKey("SomeConnectionKey333"); };

            _listener.PeerDisconnectedEvent += (peer, info) =>
            {
                _endPointReceivers.TryRemove(peer.EndPoint, out var _);
                onDisconnect(peer.EndPoint, BuildDisconnectInfo(info));
            };

            _listener.NetworkReceiveEvent += (peer, dataReader, method) =>
            {
                var dataPacket = new DataPacket(dataReader.RawData, dataReader.UserDataOffset, dataReader.UserDataSize,
                    ConvertDeliveryMethod(method));
                onReceivePacket(peer.EndPoint, dataPacket, dataReader.Recycle);
            };
            
            _listener.PeerConnectedEvent += peer =>
            {
                onConnect(peer.EndPoint);
                _endPointReceivers.TryAdd(peer.EndPoint, peer);
            };
        }

        private static DeliveryOptions ConvertDeliveryMethod(DeliveryMethod method)
        {
            switch (method)
            {
                case DeliveryMethod.Unreliable:
                    return new DeliveryOptions(false,false);
                    break;
                case DeliveryMethod.ReliableUnordered:
                    return new DeliveryOptions(true,false);
                    break;
                case DeliveryMethod.ReliableOrdered:
                    return new DeliveryOptions(true,false);
                    break;
                default:
                    throw new NotSupportedException($"Delivery method {method} not supported");
            }
        }

        public void Listen(int port)
        {
            _peer.Start(port);
        }



        public void Tick()
        {
            _peer.PollEvents();
        }

        private DeliveryMethod GetDeliveryMode(bool reliable, bool orderControl)
        {
            var mode = DeliveryMethod.Unreliable;
            if (reliable)
                mode = DeliveryMethod.ReliableUnordered;
            if (reliable && orderControl)
                mode = DeliveryMethod.ReliableOrdered;
            return mode;
        }
        
        public void Send(byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true)
        {
            if (_serverPeer == null)
                _logger.Error($"Server peer is null");
            
            _serverPeer?.Send(buffer, offset, length, GetDeliveryMode(reliable, orderControl));
        }

        // private bool debugLogSent = false;

        public void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl,
            bool returnAfterSend = true)
        {
            if (_endPointReceivers.TryGetValue(endPoint, out var connection))
            {
                var deliveryMethod = GetDeliveryMode(reliable, orderControl);

                // todo make PR to LiteNet to control MTU value during it calculation
                if (length > connection.GetMaxSinglePacketSize(deliveryMethod))
                {
                    deliveryMethod = DeliveryMethod.ReliableUnordered;

                    // // todo short-time DEBUG
                    // if (length > 1200 && !debugLogSent)
                    // {
                    //     debugLogSent = true;// to avoid log pollution
                    //     _logger.Error($"TOO BIG PACKET DETECTED ({length}/{connection.GetMaxSinglePacketSize(deliveryMethod)}): {Convert.ToBase64String(buffer, offset, length)}");
                    // }
                }

                // todo Log overriding reliable flag
                // _logger.Error($"reliable {reliable} (mto: {connection.Mtu}, packet: {length})");

                connection.Send(buffer, offset, length, deliveryMethod);
            }
        }

        public int GetPing()
        {
            if (_serverPeer == null)
                return 0;
            return _serverPeer.Ping;
        }
        
        public int GetRtt()
        {
            if (_serverPeer == null)
                return 0;
            return _serverPeer.Ping * 2;
        }

        public int Mtu => _serverPeer?.Mtu ?? 0;
        public void Close()
        {
            _peer.Stop();
            _peer.DisconnectAll();
            _serverPeer?.Disconnect();
            _endPointReceivers.Clear();
        }

        public void ReturnBufferToPool(byte[] buffer)
        {
            //nothing
        }

        public bool DisconnectPeer(IPEndPoint ipEndPoint)
        {
            if (_endPointReceivers.TryGetValue(ipEndPoint, out var connection))
            {
                connection.Disconnect();
                return true;
            }

            return false;
        }

        public event Action<IPEndPoint, DataPacket, Action> OnPacketReceived;
        public event Action<IPEndPoint> OnConnected;
        public event Action<IPEndPoint, IDisconnectInfo> OnDisconnected;
    }
}