using System.Threading.Tasks;

namespace Shaman.ServerSharedUtilities
{
    public interface IBundleInfoProvider
    {
        Task<string> GetBundleUri();
    }
}