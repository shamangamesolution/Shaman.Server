using Shaman.Common.Utils.Peers;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Common.Utils.Senders
{
    public interface IShamanMessageSender
    {
        int Send(MessageBase message, IPeerSender peer);
        void CleanupPeerData(IPeerSender peer);
    }
}