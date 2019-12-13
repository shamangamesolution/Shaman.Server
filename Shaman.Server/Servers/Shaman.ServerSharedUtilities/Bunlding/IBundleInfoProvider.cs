using System.Threading.Tasks;

namespace Shaman.ServerSharedUtilities.Bunlding
{
    public interface IBundleInfoProvider
    {
        Task<string> GetBundleUri();
    }
}