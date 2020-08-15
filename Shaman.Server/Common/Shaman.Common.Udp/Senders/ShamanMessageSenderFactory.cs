using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

namespace Shaman.Common.Udp.Senders
{
    public class ShamanMessageSenderFactory : IShamanMessageSenderFactory
    {
        private readonly ISerializer _serializer;
        private readonly IPacketSenderConfig _config;
        private readonly IShamanLogger _logger;

        public ShamanMessageSenderFactory(ISerializer serializer, IPacketSenderConfig config, IShamanLogger logger)
        {
            _serializer = serializer;
            _config = config;
            _logger = logger;
        }

        public IShamanMessageSender Create(IPacketSender packetSender)
        {
            return new ShamanMessageSender(new ShamanSender(_serializer, packetSender, _logger, _config));
        }
    }
}