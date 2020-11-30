using System.Threading.Tasks;

namespace Shaman.ServerSharedUtilities.Bundling
{
    public interface IBundleInfoProvider
    {
        Task<string> GetBundleUri();
    }
}