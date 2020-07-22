using Shaman.Common.Contract;
using Shaman.Common.Utils.Peers;

namespace Shaman.Common.Utils.Senders
{
    public interface IPacketSender
    {
        void AddPacket(IPeerSender peer, DeliveryOptions deliveryOptions, Payload payload1, Payload payload2);
        void AddPacket(IPeerSender peer, DeliveryOptions deliveryOptions, Payload payload);
        void CleanupPeerData(IPeerSender peer);
        int GetMaxQueueSIze();
        int GetAverageQueueSize();
        void Start(bool shortLiving = false);
        void Stop();
    }
}