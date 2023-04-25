namespace Shaman.Common.Udp.Sockets
{
    public interface IServerTransportLayerFactory
    {
        ITransportLayer GetLayer(string protocol);
    }
}