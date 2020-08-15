namespace Shaman.Common.Udp.Senders
{
    public interface IPacketSenderConfig
    {
        int GetBasePacketBufferSize();
        int GetMaxPacketSize();
        int GetSendTickTimerMs();
    }
}