using System;

namespace Shaman.TestTools.ClientPeers
{
    public class TestClientPeerException: Exception
    {
        public TestClientPeer Peer { get; }

        public TestClientPeerException(TestClientPeer peer, string msg):base(msg)
        {
            Peer = peer;
        }
    }
}