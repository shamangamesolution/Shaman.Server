using System.Threading.Tasks;

namespace Shaman.Bundling.Common
{
    public interface IBundleInfoProvider
    {
        Task<string> GetBundleUri();
    }

    public class DefaultBundleInfoProvider : IBundleInfoProvider
    {
        public Task<string> GetBundleUri()
        {
            throw new System.NotImplementedException();
        }
    }
}