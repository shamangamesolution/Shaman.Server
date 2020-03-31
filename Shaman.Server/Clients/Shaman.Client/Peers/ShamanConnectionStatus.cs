namespace Shaman.Client.Peers
{
    public class ShamanConnectionStatus
    {
        public readonly ShamanClientStatus Status;
        public readonly string Error;
        public bool IsSuccess { get; }

        public ShamanConnectionStatus(ShamanClientStatus status, bool isSuccess = true,  string error = "")
        {
            Status = status;
            Error = error;
            IsSuccess = isSuccess;
        }
    }
}