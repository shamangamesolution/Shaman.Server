namespace Shaman.Client.Peers
{
    public interface IShamanClientPeerConfig
    {
        /// <summary>
        /// Recommended for unity client 50
        /// </summary>
        int PollPackageQueueIntervalMs { get; }
        /// <summary>
        /// Recommended for unity client false
        /// </summary>
        bool StartOtherThreadMessageProcessing { get; }
        /// <summary>
        /// Recommended for unity client 300
        /// </summary>
        int MaxPacketSize { get; }
        /// <summary>
        /// Recommended for unity client 33
        /// </summary>
        int SendTickMs { get; }
    }
}