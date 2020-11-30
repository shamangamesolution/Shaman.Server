namespace Shaman.Common.Udp.Senders
{
    public interface IPacketSenderConfig
    {
        int MaxPacketSize { get; set; }
        int BasePacketBufferSize { get; set; }
        int SendTickTimeMs { get; set; }
    }
}