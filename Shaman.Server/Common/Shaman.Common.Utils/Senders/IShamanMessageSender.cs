using System.Collections.Generic;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Common.Utils.Senders
{
    public interface IShamanMessageSender
    {
        int Send(MessageBase message, IPeerSender peer);
        int Send(MessageBase message, IEnumerable<IPeerSender> peer);
        void CleanupPeerData(IPeerSender peer);
    }
}