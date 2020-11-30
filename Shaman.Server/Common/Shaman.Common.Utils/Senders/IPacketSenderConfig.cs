namespace Shaman.Common.Utils.Senders
{
    public interface IPacketSenderConfig
    {
        int GetBasePacketBufferSize();
        int GetMaxPacketSize();
        int GetSendTickTimerMs();
    }
}