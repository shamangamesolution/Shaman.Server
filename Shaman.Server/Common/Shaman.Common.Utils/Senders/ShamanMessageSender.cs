using System.Collections.Generic;
using Shaman.Common.Contract;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Common.Utils.Senders
{
    public class ShamanMessageSender : IShamanMessageSender
    {
        private readonly IShamanSender _shamanSender;

        public ShamanMessageSender(IShamanSender shamanSender)
        {
            _shamanSender = shamanSender;
        }

        public int Send(MessageBase message, IPeerSender peer)
        {
            return _shamanSender.Send(message, new DeliveryOptions(message.IsReliable, message.IsOrdered), peer);
        }

        public int Send(MessageBase message, IEnumerable<IPeerSender> peers)
        {
            return _shamanSender.Send(message, new DeliveryOptions(message.IsReliable, message.IsOrdered), peers);
        }

        public void CleanupPeerData(IPeerSender peer)
        {
            _shamanSender.CleanupPeerData(peer);
        }
    }
}