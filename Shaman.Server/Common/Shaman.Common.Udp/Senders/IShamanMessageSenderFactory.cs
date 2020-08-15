namespace Shaman.Common.Udp.Senders
{
    public interface IShamanMessageSenderFactory
    {
        IShamanMessageSender Create(IPacketSender packetSender);
    }
}