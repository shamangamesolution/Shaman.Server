namespace Shaman.Game.Contract
{
    public interface IBackendProvider
    {
        string GetFirstBackendUrl();
        string GetBackendUrl(int id);
        void Start();
    }
}