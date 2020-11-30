using Shaman.Game.Contract;

namespace Shaman.ServerSharedUtilities.Backends
{
    public class BackendProviderStub: IBackendProvider
    {
        private const string StubMessage = "BackendProviderStub should not be called in Standalone mode";

        public string GetFirstBackendUrl()
        {
            throw new System.NotImplementedException(StubMessage);
        }

        public string GetBackendUrl(int id)
        {
            throw new System.NotImplementedException(StubMessage);
        }

        public void Start()
        {
        }
    }
}