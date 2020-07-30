using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Senders;

namespace Shaman.MM.Tests.Fakes
{
    public class FakePacketSender : IPacketSender
    {
        public int AddPacket(MessageBase message, IPeerSender peer)
        {
            return 0;
        }

        public int AddPacket(MessageBase message, IEnumerable<IPeerSender> peer)
        {
            throw new System.NotImplementedException();
        }

        public void AddPacket(IPeerSender peer, byte[] data, bool isReliable, bool isOrdered)
        {
        }

        public void PeerDisconnected(IPeerSender peer)
        {
            throw new System.NotImplementedException();
        }

        public int GetMaxQueueSIze()
        {
            throw new System.NotImplementedException();
        }

        public int GetAverageQueueSize()
        {
            throw new System.NotImplementedException();
        }

        public void Start(bool shortLiving = false)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public void AddPacket(IPeerSender peer, byte[] data, int offset, int length, bool isReliable,
            bool isOrdered)
        {
            throw new System.NotImplementedException();
        }

        public void AddPacket(IEnumerable<IPeerSender> peers, byte[] data, int offset, int length, bool isReliable, bool isOrdered)
        {
            throw new System.NotImplementedException();
        }
    }
}