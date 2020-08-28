using Shaman.Routing.Balancing.Client;

namespace Shaman.Launchers.MM.Balancing
{
    public class PeerCountProvider : IPeerCountProvider
    {
        public int GetPeerCount()
        {
            return -1;
        }
    }
}