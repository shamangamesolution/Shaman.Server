namespace Code.Network.WebRequesters
{
    public class WebRequesterConfigProviderHardcoded : IWebRequesterConfigProvider
    {
        int IWebRequesterConfigProvider.RequestTimeoutMilliseconds => 2000;

        int IWebRequesterConfigProvider.RequestStreamTimeoutMilliseconds => 2000;

        bool IWebRequesterConfigProvider.EnableThrowingExceptions => false;
    }
}