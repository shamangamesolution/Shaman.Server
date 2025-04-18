using System;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Client.Peers
{
    public class EventHandler<TOpCode>
    {
        public readonly Action<IOperationCodeProvider<TOpCode>, Exception> Handler;
        public readonly bool CallOnce;

        public EventHandler(Action<IOperationCodeProvider<TOpCode>, Exception> handler, bool callOnce)
        {
            Handler = handler;
            CallOnce = callOnce;
        }
    }
}