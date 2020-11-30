namespace Code.Network.WebRequesters
{
    public interface IWebRequesterConfigProvider
    {
        int RequestTimeoutMilliseconds { get; }
        int RequestStreamTimeoutMilliseconds { get;  }
        bool EnableThrowingExceptions { get; }
    }
}