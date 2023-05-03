using System;
using System.Net;
using Shaman.Contract.Common;

namespace Shaman.Common.Udp.Sockets
{
    public enum ShamanDisconnectReason
    {
        PeerLeave,
        ConnectionLost
    }

    public interface IDisconnectInfo : IDisposable
    {
        ShamanDisconnectReason Reason { get; }
        Payload Payload { get; }
    }

    public class SimpleDisconnectInfo : IDisconnectInfo
    {
        public SimpleDisconnectInfo(ShamanDisconnectReason reason)
        {
            Reason = reason;
            Payload = default;
        }

        public SimpleDisconnectInfo(ShamanDisconnectReason reason, Payload payload)
        {
            Reason = reason;
            Payload = default;
        }

        public void Dispose()
        {
        }

        public ShamanDisconnectReason Reason { get; }
        public Payload Payload { get; }
    }

    public interface ITransportLayer
    {
        //todo split it, finally

        #region client

        void Connect(IPEndPoint endPoint);
        void Send(byte[] buffer, int offset, int length, bool reliable, bool orderControl, bool returnAfterSend = true);

        int GetPing();
        int GetRtt();
        int Mtu { get; }
        void Close();
        void Close(byte[] data, int offset, int length);
        
        event Action<IPEndPoint, DataPacket, Action> OnPacketReceived;
        event Action<IPEndPoint> OnConnected;
        event Action<IPEndPoint, IDisconnectInfo> OnDisconnected;

        #endregion

        #region server

        // todo merge AddEventCallbacks and Listen
        void AddEventCallbacks(Action<IPEndPoint, DataPacket, Action> onReceivePacket, Func<IPEndPoint, bool> onConnect,
            Action<IPEndPoint, IDisconnectInfo> onDisconnect);
        void Listen(int port);
        void Tick();
        bool IsTickRequired { get; }
        void Send(IPEndPoint endPoint, byte[] buffer, int offset, int length, bool reliable, bool orderControl);
        bool DisconnectPeer(IPEndPoint ipEndPoint);
        bool DisconnectPeer(IPEndPoint ipEndPoint, byte[] data, int offset, int length);

        #endregion
    }
}