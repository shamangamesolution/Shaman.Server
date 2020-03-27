using Shaman.Common.Utils.Sockets;

namespace Shaman.Common.Utils.Peers
{
    public interface IPeerSender
    {
        //void Send(MessageBase message);
        //void Send(byte[] bytes, bool isReliable, bool isOrdered);
        void Send(PacketInfo packetInfo);
    }
}