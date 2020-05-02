using Shaman.Client.Peers;

namespace Code.Network
{
    public class UnityClientPeerConfig : IShamanClientPeerConfig
    {


        public int PollPackageQueueIntervalMs { get; }
        public bool StartOtherThreadMessageProcessing { get; }
        public int MaxPacketSize { get; }
        public int SendTickMs { get; }
    
        public UnityClientPeerConfig(int pollPackageQueueIntervalMs, bool startOtherThreadMessageProcessing, int maxPacketSize, int sendTickMs)
        {
            PollPackageQueueIntervalMs = pollPackageQueueIntervalMs;
            StartOtherThreadMessageProcessing = startOtherThreadMessageProcessing;
            MaxPacketSize = maxPacketSize;
            SendTickMs = sendTickMs;
        }
    }
}
