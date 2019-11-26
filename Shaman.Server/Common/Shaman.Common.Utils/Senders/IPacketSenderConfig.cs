namespace Shaman.Common.Utils.Senders
{
    public interface IPacketSenderConfig
    {
        int GetMaxPacketSize();
        int GetSendTickTimerMs();
    }
}