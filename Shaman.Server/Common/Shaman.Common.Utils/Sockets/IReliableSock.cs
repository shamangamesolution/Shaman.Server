using System;
using System.Net;

namespace Shaman.Common.Utils.Sockets
{
    public interface IReliableSock
    {
        void Connect(IPEndPoint endPoint);
        void Send(byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true);

        void AddEventCallbacks(Action<IPEndPoint, DataPacket, Action> onReceivePacket, Action<IPEndPoint> onConnect,
            Action<IPEndPoint, string> onDisconnect);
        void Listen(int port);
        void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true);

        int GetPing();
        int GetRtt();
        
        void Tick();
        void Close();
        void ReturnBufferToPool(byte[] buffer);
        
        event Action<IPEndPoint, DataPacket, Action> OnPacketReceived;
        event Action<IPEndPoint> OnConnected;
        event Action<IPEndPoint, string> OnDisconnected;
        int Mtu { get; }
        bool DisconnectPeer(IPEndPoint ipEndPoint);
    }
}