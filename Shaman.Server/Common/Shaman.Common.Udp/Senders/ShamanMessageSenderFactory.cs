using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

namespace Shaman.Common.Udp.Senders
{
    public class ShamanMessageSenderFactory : IShamanMessageSenderFactory
    {
        private readonly ISerializer _serializer;
        private readonly IPacketSenderConfig _config;

        public ShamanMessageSenderFactory(ISerializer serializer, IPacketSenderConfig config)
        {
            _serializer = serializer;
            _config = config;
        }

        public IShamanMessageSender Create(IPacketSender packetSender)
        {
            return new ShamanMessageSender(new ShamanSender(_serializer, packetSender, _config));
        }
    }
}