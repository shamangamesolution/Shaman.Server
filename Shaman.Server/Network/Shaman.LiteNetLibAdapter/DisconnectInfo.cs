using LiteNetLib;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils;
using Shaman.Contract.Common;

namespace Shaman.LiteNetLibAdapter
{
    public class LiteNetDisconnectInfo : OnceDisposable, IDisconnectInfo
    {
        private readonly NetPacketReader _payload;

        public LiteNetDisconnectInfo(DisconnectInfo info)
        {
            _payload = info.Reason == DisconnectReason.RemoteConnectionClose ? info.AdditionalData : null;
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

        protected override void DisposeImpl()
        {
            _payload?.Recycle();
        }

        public ShamanDisconnectReason Reason { get; }

        public Payload Payload => _payload != null
            ? new Payload(_payload.RawData, _payload.UserDataOffset, _payload.UserDataSize)
            : default;
    }
}