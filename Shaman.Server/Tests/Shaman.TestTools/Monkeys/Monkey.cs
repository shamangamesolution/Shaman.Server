using System;
using Shaman.Client;
using Shaman.Client.Peers;


namespace Shaman.TestTools.Monkeys
{
    public class Monkey
    {
        public IShamanClientPeer  Peer { get; set; }
        public Route Route { get; set; }
        public Guid SessionId { get; set; }
        public string GuestId { get; set; }
    }
}