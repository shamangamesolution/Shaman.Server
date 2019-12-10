using System.Threading.Tasks;

namespace Shaman.Game.Providers
{
    public interface IBundleInfoProvider
    {
        Task<string> GetBundleUri();
    }
}