using System.Collections.Generic;
using Shaman.Common.Utils.Peers;

namespace Shaman.Common.Utils.Senders
{
    public interface IPacketSender
    {
        void AddPacket(IPeerSender peer, byte[] data, int offset, int length, bool isReliable, bool isOrdered);
        void PeerDisconnected(IPeerSender peer);
        int GetMaxQueueSIze();
        int GetAverageQueueSize();
        void Start(bool shortLiving = false);
        void Stop();
    }
}