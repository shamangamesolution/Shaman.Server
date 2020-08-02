using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Peers;
using Shaman.Common.Utils.Serialization;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.Common.Utils.Senders
{
    public interface IShamanSender
    {
        int Send(ISerializable message, DeliveryOptions deliveryOptions, IPeerSender peer);
        int Send(ISerializable message, DeliveryOptions deliveryOptions, IEnumerable<IPeerSender> peers);
        void CleanupPeerData(IPeerSender peer);
    }

    public class ShamanSender : ShamanSenderBase<IPeerSender>, IShamanSender
    {
        private readonly IPacketSender _packetSender;
        
        private static readonly ConcurrentDictionary<Type, int> BufferStatistics = new ConcurrentDictionary<Type, int>();

        public ShamanSender(ISerializer serializer, IPacketSender packetSender, IShamanLogger logger,
            IPacketSenderConfig config): base(serializer, logger, config.GetBasePacketBufferSize())
        {
            _packetSender = packetSender;
        }
        protected override void Send(DeliveryOptions deliveryOptions, IPeerSender peer, Payload payload)
        {
            _packetSender.AddPacket(peer, deliveryOptions, payload);
        }

        public void CleanupPeerData(IPeerSender peer)
        {
            _packetSender.CleanupPeerData(peer);
        }
    }
}