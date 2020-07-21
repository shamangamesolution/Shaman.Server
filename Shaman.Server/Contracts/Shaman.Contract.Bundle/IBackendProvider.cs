namespace Shaman.Contract.Bundle
{
    public interface IBackendProvider
    {
        string GetFirstBackendUrl();
        string GetBackendUrl(int id);
        void Start();
    }
}