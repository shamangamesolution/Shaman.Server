using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Udp.Sockets
{
    public interface ISocketFactory
    {
        IReliableSock GetReliableSockWithBareSocket(IShamanLogger logger);
        IReliableSock GetReliableSockWithThreadSocket(IShamanLogger logger);
    }
}