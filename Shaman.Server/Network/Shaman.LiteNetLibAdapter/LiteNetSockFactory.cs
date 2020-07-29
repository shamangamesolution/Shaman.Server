using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Sockets;

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