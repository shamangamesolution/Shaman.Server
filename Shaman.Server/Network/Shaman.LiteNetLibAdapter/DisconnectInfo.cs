using LiteNetLib;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common;

namespace Shaman.LiteNetLibAdapter
{
    public class LiteNetDisconnectInfo : IDisconnectInfo
    {
        private readonly NetPacketReader _payload;

        public LiteNetDisconnectInfo(DisconnectInfo info)
        {
            _payload = info.AdditionalData;
            Reason = ConvertReason(info.Reason);
        }

        private ShamanDisconnectReason ConvertReason(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.RemoteConnectionClose:
                case DisconnectReason.DisconnectPeerCalled:
                    return ShamanDisconnectReason.PeerLeave;
                default:
                    return ShamanDisconnectReason.ConnectionLost;
            }
        }

        public void Dispose()
        {
            _payload?.Recycle();
        }

        public ShamanDisconnectReason Reason { get; }

        public Payload Payload => _payload != null
            ? new Payload(_payload.RawData, _payload.UserDataOffset, _payload.UserDataSize)
            : default;
    }
}