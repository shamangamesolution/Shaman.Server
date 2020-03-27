namespace Shaman.Messages.Handling
{
    public interface IMessageHandlersRegistry<out TContext>
    {
        void RegisterHandler(ushort messageCode, IMessageDataHandler<TContext> messageHandler);
    }
}