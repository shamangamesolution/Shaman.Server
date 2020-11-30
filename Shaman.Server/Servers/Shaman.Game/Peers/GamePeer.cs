using Shaman.Common.Server.Peers;

namespace Shaman.Game.Peers
{
    public class GamePeer : Peer
    {
        public bool IsAuthorized { get; set; }
        public bool IsAuthorizing { get; set; }
    }
}