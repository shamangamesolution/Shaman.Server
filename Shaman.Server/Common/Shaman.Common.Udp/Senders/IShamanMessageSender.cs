using Shaman.Common.Udp.Peers;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Common.Udp.Senders
{
    public interface IShamanMessageSender
    {
        int Send(MessageBase message, IPeerSender peer);
        void CleanupPeerData(IPeerSender peer);
    }
}