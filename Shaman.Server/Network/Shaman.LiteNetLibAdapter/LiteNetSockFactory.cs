using Shaman.Common.Utils.Sockets;
using Shaman.Contract.Common.Logging;

namespace Shaman.LiteNetLibAdapter
{
    public class LiteNetSockFactory : ISocketFactory
    {
        public IReliableSock GetReliableSockWithBareSocket(IShamanLogger logger)
        {
            return new LiteNetSock(logger);
        }

        public IReliableSock GetReliableSockWithThreadSocket(IShamanLogger logger)
        {
            return new LiteNetSock(logger);
        }
    }
}