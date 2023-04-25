using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Udp.Sockets
{
    public interface IClientTransportLayerFactory
    {
        ITransportLayer Create(IShamanLogger logger);
    }
}