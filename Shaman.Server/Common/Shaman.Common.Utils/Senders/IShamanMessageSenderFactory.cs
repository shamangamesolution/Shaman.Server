namespace Shaman.Common.Utils.Senders
{
    public interface IShamanMessageSenderFactory
    {
        IShamanMessageSender Create(IPacketSender packetSender);
    }
}