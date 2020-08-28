using Shaman.Routing.Balancing.Client;

namespace Shaman.Launchers.Game.Balancing
{
    public class PeerCountProvider : IPeerCountProvider
    {
        public int GetPeerCount()
        {
            return -1;
        }
    }
}