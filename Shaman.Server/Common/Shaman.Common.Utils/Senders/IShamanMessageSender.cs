using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Peers;

namespace Shaman.Common.Utils.Senders
{
    public interface IShamanMessageSender
    {
        int AddPacket(MessageBase message, IPeerSender peer);
        int AddPacket(MessageBase message, IEnumerable<IPeerSender> peer);
        void CleanupPeerData(IPeerSender peer);
    }
}