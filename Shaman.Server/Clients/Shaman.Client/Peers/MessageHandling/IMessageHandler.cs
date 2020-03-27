using System;
using Shaman.Common.Utils.Messages;

namespace Shaman.Client.Peers.MessageHandling
{
    public interface IMessageHandler
    {
        Guid RegisterOperationHandler<T>(Action<T> handler,
            bool callOnce = false) where T : MessageBase, new();

        bool UnregisterOperationHandler(Guid id);
        bool ProcessMessage(ushort operationCode, byte[] buffer, int offset, int length);
    }
}