namespace Shaman.Messages.Handling
{
    public interface IMessageHandlersRegistry<out TContext>
    {
        void RegisterHandler(ushort messageCode, byte messageType, IMessageDataHandler<TContext> messageHandler);
    }
}