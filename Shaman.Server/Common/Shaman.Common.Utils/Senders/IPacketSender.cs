using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Peers;

namespace Shaman.Common.Utils.Senders
{
    public interface IPacketSender
    {
        int AddPacket(MessageBase message, IPeerSender peer);
        int AddPacket(MessageBase message, IEnumerable<IPeerSender> peer);
        void AddPacket(IPeerSender peer, byte[] data, bool isReliable, bool isOrdered);
        void PeerDisconnected(IPeerSender peer);
        int GetMaxQueueSIze();
        int GetAverageQueueSize();
        void Start(bool shortLiving = false);
        void Stop();
        void AddPacket(IPeerSender peer, byte[] data, int offset, int length, bool isReliable, bool isOrdered);
        void AddPacket(IEnumerable<IPeerSender> peers, byte[] data, int offset, int length, bool isReliable, bool isOrdered);
    }
}