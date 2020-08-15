using Shaman.Common.Udp.Peers;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.Serialization.Utils.Pooling;

namespace Shaman.Common.Udp.Senders
{
    public interface IShamanSender
    {
        int Send(ISerializable message, DeliveryOptions deliveryOptions, IPeerSender peer);
        void CleanupPeerData(IPeerSender peer);
    }

    public class ShamanSender : IShamanSender
    {
        private readonly ISerializer _serializer;
        private readonly IPacketSender _packetSender;
        private readonly ShamanStreamPool _shamanStreamPool;

        public ShamanSender(ISerializer serializer, IPacketSender packetSender, IShamanLogger logger,
            IPacketSenderConfig config)
        {
            _shamanStreamPool = new ShamanStreamPool(config.GetBasePacketBufferSize());
            _serializer = serializer;
            _packetSender = packetSender;
        }

        public int Send(ISerializable message, DeliveryOptions deliveryOptions, IPeerSender peer)
        {
            var stream = _shamanStreamPool.Rent(message.GetType());
            try
            {
                _serializer.Serialize(message, stream);
                _packetSender.AddPacket(peer, deliveryOptions, new Payload(stream.GetBuffer()));
                return (int) stream.Length;
            }
            finally
            {
                _shamanStreamPool.Return(stream, message.GetType());
            }
        }

        public void CleanupPeerData(IPeerSender peer)
        {
            _packetSender.CleanupPeerData(peer);
        }
    }
}