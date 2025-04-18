namespace Shaman.Serialization.Messages.Udp
{
    public interface IUdpOptions
    {
        bool IsReliable { get; }
        bool IsOrdered { get; }
    }
}