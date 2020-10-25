using Shaman.Client.Peers;
using Shaman.Contract.Common.Logging;

namespace Shaman.TestTools.ClientPeers
{
    public class TestClientPeerListener : IShamanClientPeerListener
    {
        private readonly IShamanLogger _logger;

        public TestClientPeerListener(IShamanLogger logger)
        {
            _logger = logger;
        }
        
        public void OnStatusChanged(ShamanClientStatus prevStatus, ShamanClientStatus newStatus)
        {
            _logger.LogInfo($"Status changed from {prevStatus} to {newStatus}");
        }
    }
}