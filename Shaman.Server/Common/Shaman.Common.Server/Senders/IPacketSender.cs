using System;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Messages;

namespace Shaman.Common.Server.Senders
{
    public interface IPacketSender
    {
        void AddPacket(MessageBase message, IPeer peer);
        void AddPacket(IPeer peer, byte[] packet, bool isReliable, bool isOrdered);
        void PeerDisconnected(Guid peerId);
        int GetMaxQueueSIze();
        int GetAverageQueueSize();
        void Send();
    }
}