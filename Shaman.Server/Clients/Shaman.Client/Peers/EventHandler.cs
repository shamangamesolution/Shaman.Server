using System;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Client.Peers
{
    public class EventHandler
    {
        public readonly Action<MessageBase> Handler;
        public readonly bool CallOnce;

        public EventHandler(Action<MessageBase> handler, bool callOnce)
        {
            Handler = handler;
            CallOnce = callOnce;
        }
    }
}