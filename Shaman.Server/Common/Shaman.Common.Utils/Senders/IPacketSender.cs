using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Peers;

namespace Shaman.Common.Utils.Senders
{
    public interface IPacketSender
    {
        void AddPacket(MessageBase message, IPeerSender peer);
        void AddPacket(IPeerSender peer, byte[] data, bool isReliable, bool isOrdered);
        void PeerDisconnected(IPeerSender peer);
        int GetMaxQueueSIze();
        int GetAverageQueueSize();
        void Start(bool shortLiving = false);
        void Stop();
    }
}