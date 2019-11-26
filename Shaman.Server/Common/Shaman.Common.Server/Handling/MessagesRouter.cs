using System;
using System.Collections;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.GameBundleContract;

namespace Shaman.Common.Server.Handling
{
    public static class MessagesRouterFactory
    {
        public static MessagesRouter<TMessageHandler> Create<TMessageHandler, TMessageDispatcher>()
            where TMessageDispatcher : MessagesDispatcherBase<TMessageHandler>, new()
        {
            var messagesProcessor = new MessagesRouter<TMessageHandler>();
            var dispatcher = new TMessageDispatcher();
            dispatcher.Initialize(messagesProcessor);
            return messagesProcessor;
        }
    }


    public class MessagesRouter<TContext> : IMessageHandlersRegistry<TContext>
    {
        private readonly Hashtable _handlersMap = new Hashtable();

        public void RegisterHandler(ushort messageCode, byte messageType,
            IMessageDataHandler<TContext> messageHandler)
        {
            _handlersMap.Add(BuildKey(messageCode, messageType), messageHandler);
        }

        private static int BuildKey(ushort messageCode, byte messageType)
        {
            return messageCode << 8 | messageType;
        }

        public MessageResult Route(ISerializer serializer, byte[] data,
            int offset, int length, Guid sessionId, TContext ctx)
        {
            var operationCode = MessageBase.GetOperationCode(data, offset);
            var messageType = MessageBase.GetMessageType(data, offset);

            var handler = (IMessageDataHandler<TContext>) _handlersMap[BuildKey(operationCode, (byte) messageType)];
            if (handler == null)
            {
                throw new MessageProcessingException(
                    $"Message with code {operationCode} and type {messageType} not supported.");
            }

            try
            {
                return handler.Handle(serializer, data, offset, length, sessionId, ctx);
            }
            catch (Exception e)
            {
                throw new MessageProcessingException(
                    $"Error processing with code {operationCode} and type {messageType}", e);
            }
        }
    }
}