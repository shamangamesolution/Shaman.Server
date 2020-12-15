using System;
using System.Net;

namespace Shaman.Common.Udp.Sockets
{
    public enum ClientDisconnectReason
    {
        PeerLeave,
        ConnectionLost
    }

    public interface IDisconnectInfo : IDisposable
    {
        ClientDisconnectReason Reason { get; }
        byte[] Payload { get; }
    }

    public class SimpleDisconnectInfo : IDisconnectInfo
    {
        public SimpleDisconnectInfo(ClientDisconnectReason reason)
        {
            Reason = reason;
            Payload = null;
        }

        public void Dispose()
        {
        }

        public ClientDisconnectReason Reason { get; }
        public byte[] Payload { get; }
    }

    public interface IReliableSock
    {
        void Connect(IPEndPoint endPoint);
        void Send(byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true);

        void AddEventCallbacks(Action<IPEndPoint, DataPacket, Action> onReceivePacket, Action<IPEndPoint> onConnect,
            Action<IPEndPoint, IDisconnectInfo> onDisconnect);
        void Listen(int port);
        void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true);

        int GetPing();
        int GetRtt();
        
        void Tick();
        void Close();
        void ReturnBufferToPool(byte[] buffer);
        
        event Action<IPEndPoint, DataPacket, Action> OnPacketReceived;
        event Action<IPEndPoint> OnConnected;
        event Action<IPEndPoint, IDisconnectInfo> OnDisconnected;
        int Mtu { get; }
        bool DisconnectPeer(IPEndPoint ipEndPoint);
        bool DisconnectPeer(IPEndPoint ipEndPoint, byte[] data, int offset, int length);
    }
}