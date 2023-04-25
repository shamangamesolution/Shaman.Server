using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Common.Logging;

namespace Shaman.LiteNetLibAdapter
{
    public class LiteNetSockFactory : IServerTransportLayerFactory
    {
        private readonly IShamanLogger _logger;

        public LiteNetSockFactory(IShamanLogger logger)
        {
            _logger = logger;
        }

        public ITransportLayer GetLayer(string protocol)
        {
            return new LiteNetSock(_logger);
        }
    }
    public class LiteNetClientTransportLayerFactory : IClientTransportLayerFactory
    {
        public ITransportLayer Create(IShamanLogger logger)
        {
            return new LiteNetSock(logger);
        }
    }
}