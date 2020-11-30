namespace Shaman.Contract.Routing.Meta
{
    public interface IMetaProvider
    {
        string GetFirstMetaServerUrl();
        string GetMetaServerUrl(int id);
        void Start(int getBackendListIntervalMs = 1000);
        void Stop();
    }}