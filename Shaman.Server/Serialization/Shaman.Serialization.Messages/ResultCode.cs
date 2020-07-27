namespace Shaman.Serialization.Messages
{
    public enum ResultCode : ushort
    {
        OK = 0,
        UnknownOperation = 1,
        MessageProcessingError = 2,
        SendRequestError = 3,
        RequestProcessingError = 4,
        NotAuthorized = 5,
        BadReceipt = 6
    }
}