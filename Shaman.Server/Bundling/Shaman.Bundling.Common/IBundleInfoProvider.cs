using System.Threading.Tasks;

namespace Shaman.Bundling.Common
{
    public interface IBundleInfoProvider
    {
        Task<string> GetBundleUri();
        Task<bool> GetToOverwriteExisting();
    }

    public class DefaultBundleInfoProvider : IBundleInfoProvider
    {
        private readonly IDefaultBundleInfoConfig _config;
        
        public DefaultBundleInfoProvider(IDefaultBundleInfoConfig config)
        {
            _config = config;
        }
        
        public async Task<string> GetBundleUri()
        {
            return _config.BundleUri;
        }

        public async Task<bool> GetToOverwriteExisting()
        {
            return _config.ToOverwriteExisting;
        }

    }
}