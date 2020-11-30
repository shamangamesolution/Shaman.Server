using Shaman.Common.Server.Peers;

namespace Shaman.MM.Peers
{
    public class MmPeer : Peer
    {
        public bool IsAuthorized { get; set; }
        public bool IsAuthorizing { get; set; }
    }
}