using System;
using System.Net;

namespace Shaman.Common.Utils.Sockets
{
    public interface IReliableSock
    {
        void Connect(IPEndPoint endPoint);
        void AddEventCallbacks(Action<PacketInfo> onReceivePacket, Action<IPEndPoint> onConnect, Action<IPEndPoint, string> onDisconnect);
        void Listen(int port);
        void Tick();
        void Send(byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true);
        void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true);
        void Close();
        void ReturnBufferToPool(byte[] buffer);
        
        event Action<PacketInfo> OnPacketReceived;
        event Action<IPEndPoint> OnConnected;
        event Action<IPEndPoint> OnReliablePacketTimeout;
        event Action<IPEndPoint, string> OnDisconnected;
    }
}