using System;
using Shaman.Serialization.Messages;

namespace Shaman.Client.Peers.MessageHandling
{
    public interface IMessageHandler<TOpCode>
    {
        Guid RegisterOperationHandler<T>(Action<T, Exception> handler, 
            bool callOnce = false) where T : IOperationCodeProvider<TOpCode>, new();

        bool UnregisterOperationHandler(Guid id);
        bool ProcessMessage(TOpCode operationCode, byte[] buffer, int offset, int length);
    }
}