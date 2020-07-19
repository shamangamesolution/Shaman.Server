using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Hazel;
using Hazel.Udp;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Sockets;

namespace Shaman.HazelAdapter
{
    //todo FIX
    // public class HazelSock : IReliableSock
    // {
    //     private UdpClientConnection _clientConnection;
    //     private UdpConnectionListener _listener;
    //     private readonly ConcurrentDictionary<IPEndPoint, Connection> _endPointReceivers = new ConcurrentDictionary<IPEndPoint, Connection>();
    //     private readonly IShamanLogger _logger;
    //     
    //     
    //     public HazelSock(IShamanLogger logger)
    //     {
    //         _logger = logger;
    //     }
    //     
    //     public void ReturnBufferToPool(byte[] buffer)
    //     {
    //         //nothing
    //     }
    //     
    //     public void AddEventCallbacks(Action<IPEndPoint, DataPacket, Action> onReceivePacket,
    //         Action<IPEndPoint> onConnect,
    //         Action<IPEndPoint, IDisconnectInfo> onDisconnect)
    //     {
    //         _listener.NewConnection += args =>
    //         {
    //             _endPointReceivers.TryAdd(args.Connection.EndPoint, args.Connection);
    //             
    //             onConnect(args.Connection.EndPoint);
    //             
    //             args.Connection.Disconnected += (sender, eventArgs) =>
    //             {
    //                 _endPointReceivers.TryRemove(args.Connection.EndPoint, out var con);
    //                 onDisconnect(args.Connection.EndPoint, eventArgs);
    //             };
    //
    //             args.Connection.DataReceived += eventArgs =>
    //             {
    //                 var dataPacket = new DataPacket(eventArgs.Message.Buffer, eventArgs.Message.Offset,
    //                     eventArgs.Message.Length, (eventArgs.SendOption & SendOption.Reliable) != 0);
    //                 onReceivePacket(args.Connection.EndPoint,dataPacket,eventArgs.Message.Recycle);
    //             };
    //         };
    //     }
    //
    //     public void Connect(IPEndPoint endPoint)
    //     {
    //         _logger?.Info($"Connect {endPoint.AddressFamily}|{endPoint.Address}:{endPoint.Port}");
    //         _clientConnection = new UdpClientConnection(endPoint);
    //         _clientConnection.DataReceived += args =>
    //         {
    //             var dataPacket = new DataPacket(args.Message.Buffer, args.Message.Offset, args.Message.Length, (args.SendOption & SendOption.Reliable) != 0);
    //             OnPacketReceived?.Invoke(endPoint, dataPacket, args.Message.Recycle);
    //         };
    //         _clientConnection.Disconnected += (sender, args) => { OnDisconnected?.Invoke(endPoint, args.Reason); }; 
    //         _clientConnection.Connect();
    //     }
    //     
    //     public void Listen(int port)
    //     {
    //         _listener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, port));
    //         _listener.Start();
    //     }
    //
    //     public void Tick()
    //     {
    //         
    //     }
    //
    //     public void Send(byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true)
    //     {
    //         var message = new ArraySegment<byte>(buffer, offset, length).ToArray();
    //         _clientConnection.SendBytes(message, reliable ? SendOption.Reliable : SendOption.None);
    //     }
    //
    //     public void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true)
    //     {
    //         var message = new ArraySegment<byte>(buffer, offset, length).ToArray();
    //         if (_endPointReceivers.TryGetValue(endPoint, out var connection))
    //             connection.SendBytes(message, reliable ? SendOption.Reliable : SendOption.None);
    //     }
    //
    //     public void Close()
    //     {
    //         _listener?.Close();
    //         _clientConnection?.Dispose();
    //         _endPointReceivers.Clear();
    //     }
    //
    //     public event Action<IPEndPoint, DataPacket, Action> OnPacketReceived;
    //     public event Action<IPEndPoint> OnConnected;
    //     public event Action<IPEndPoint> OnReliablePacketTimeout;
    //     public event Action<IPEndPoint, IDisconnectInfo> OnDisconnected;
    // }
}