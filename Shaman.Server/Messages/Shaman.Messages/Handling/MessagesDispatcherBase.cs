using Shaman.Common.Utils.Messages;

namespace Shaman.Messages.Handling
{
    public abstract class MessagesDispatcherBase<TContext> 
    {
        private IMessageHandlersRegistry<TContext> _messageHandlersRegistry;

        public void Initialize(IMessageHandlersRegistry<TContext> messageHandlersRegistry)
        {
            _messageHandlersRegistry = messageHandlersRegistry;
            Initialize();
        }

        protected abstract void Initialize();
        
        public void RegisterHandler<TMessage>(IMessageHandler<TMessage, TContext> handler)
            where TMessage : MessageBase, new()
        {
            var messageBase = new TMessage();
            _messageHandlersRegistry.RegisterHandler(messageBase.OperationCode, (byte) messageBase.Type,
                MessageHandler<TMessage, TContext>.Get(handler));
        }
    }
}