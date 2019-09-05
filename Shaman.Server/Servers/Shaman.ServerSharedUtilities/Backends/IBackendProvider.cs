namespace Shaman.ServerSharedUtilities.Backends
{
    public interface IBackendProvider
    {
        string GetFirstBackendUrl();
        string GetBackendUrl(int id);
        void Start();
    }
}