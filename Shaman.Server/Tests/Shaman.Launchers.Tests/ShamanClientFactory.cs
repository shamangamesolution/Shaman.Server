using Shaman.Client.Peers;

namespace Shaman.Launchers.Tests
{
    public class ClientPeerConfig : IShamanClientPeerConfig
    {
        public int PollPackageQueueIntervalMs => 10;
        public bool StartOtherThreadMessageProcessing => true;
        public int MaxPacketSize => 300;
        public int SendTickMs => 30;
    }
}