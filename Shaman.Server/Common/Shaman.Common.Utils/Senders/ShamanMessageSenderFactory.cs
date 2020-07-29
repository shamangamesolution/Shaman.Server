using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;

namespace Shaman.Common.Utils.Senders
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