using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Utils.Logging;

namespace Shaman.Common.Utils.Sockets
{
    public interface ISocketFactory
    {
        IReliableSock GetReliableSockWithBareSocket(IShamanLogger logger);
        IReliableSock GetReliableSockWithThreadSocket(IShamanLogger logger);
    }
}