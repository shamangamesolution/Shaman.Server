using Shaman.Common.Udp.Sockets;

namespace Shaman.Common.Udp.Peers
{
    public interface IPeerSender
    {
        void Send(PacketInfo packetInfo);
    }
}