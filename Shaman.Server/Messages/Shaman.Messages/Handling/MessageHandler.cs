using System;
using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.Handling
{
    public interface IMessageHandler<in TMessage, in TContext>
    {
        bool Handle(TMessage message, Guid sessionId, TContext ctx);
    }

    public interface IMessageDataHandler<in TContext>
    {
        MessageResult Handle(ISerializer serializer, byte[] data, int offset, int length, Guid sessionId,
            TContext ctx);
    }

    public sealed class MessageHandler<TMessage, TContext> : IMessageDataHandler<TContext>
        where TMessage : MessageBase, new()
    {
        private readonly IMessageHandler<TMessage, TContext> _handler;
        public ushort Type { get; set; }

        private MessageHandler(IMessageHandler<TMessage, TContext> handler)
        {
            _handler = handler;
        }

        public static IMessageDataHandler<TContext> Get(IMessageHandler<TMessage, TContext> handler)
        {
            return new MessageHandler<TMessage, TContext>(handler);
        }

        public MessageResult Handle(ISerializer serializer, byte[] data, int offset,
            int length, Guid sessionId, TContext ctx)
        {
            var message = serializer.DeserializeAs<TMessage>(data, offset, length);
            var handle = _handler.Handle(message, sessionId, ctx);
            return new MessageResult
            {
                DeserializedMessage = message,
                Handled = handle
            };
        }
    }
}