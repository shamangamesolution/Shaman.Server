using System;
using System.Collections;
using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;

namespace Shaman.Messages.Handling
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

        public void RegisterHandler(ushort messageCode,
            IMessageDataHandler<TContext> messageHandler)
        {
            _handlersMap.Add(BuildKey(messageCode), messageHandler);
        }

        private static int BuildKey(ushort messageCode)
        {
            return messageCode;
        }

        public MessageResult Route(ISerializer serializer, ushort opCode, byte[] data,
            int offset, int length, Guid sessionId, TContext ctx)
        {
            var handler = (IMessageDataHandler<TContext>) _handlersMap[BuildKey(opCode)];
            if (handler == null)
            {
                throw new MessageProcessingException(
                    $"Message with code {opCode}  not supported.");
            }

            try
            {
                return handler.Handle(serializer, data, offset, length, sessionId, ctx);
            }
            catch (Exception e)
            {
                throw new MessageProcessingException(
                    $"Error processing with code {opCode}", e);
            }
        }
    }
}