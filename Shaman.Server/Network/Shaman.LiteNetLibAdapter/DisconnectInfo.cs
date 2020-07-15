using LiteNetLib;
using Shaman.Common.Utils.Sockets;

namespace Shaman.LiteNetLibAdapter
{
    public class LightNetDisconnectInfo : IDisconnectInfo
    {
        private readonly NetPacketReader _payload;

        public LightNetDisconnectInfo(ClientDisconnectReason reason)
        {
            _payload = null;
            Reason = reason;
        }
        public LightNetDisconnectInfo(DisconnectReason reason, NetPacketReader payload)
        {
            _payload = payload;
            Reason = ConvertReason(reason);
        }

        private ClientDisconnectReason ConvertReason(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.RemoteConnectionClose:
                case DisconnectReason.DisconnectPeerCalled:
                    return ClientDisconnectReason.PeerLeave;
                default:
                    return ClientDisconnectReason.ConnectionLost;
            }
        }

        public LightNetDisconnectInfo(DisconnectReason reason)
        {
            _payload = null;
            Reason = ConvertReason(reason);
        }

        public void Dispose()
        {
            _payload?.Recycle();
        }

        public ClientDisconnectReason Reason { get; }
        public byte[] Payload => _payload?.RawData;
    }
}