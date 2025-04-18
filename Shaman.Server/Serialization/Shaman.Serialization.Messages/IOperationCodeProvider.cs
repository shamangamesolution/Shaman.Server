namespace Shaman.Serialization.Messages
{
    public interface IOperationCodeProvider<T> : ISerializable
    {
        T OperationCode { get; }
    }
}