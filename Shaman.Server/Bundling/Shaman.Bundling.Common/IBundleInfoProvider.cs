using System.Threading.Tasks;

namespace Shaman.Bundling.Common
{
    public interface IBundleInfoProvider
    {
        Task<string> GetBundleUri();
    }

    public class DefaultBundleInfoProvider : IBundleInfoProvider
    {
        public async Task<string> GetBundleUri()
        {
            return "/Users/***REMOVED***/dev/builds/Shaman.Publish/Test/Game";
        }
    }
}