using Shaman.Common.Utils.Logging;
using Shaman.ServerSharedUtilities;

namespace Shaman.Router
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            Bootstrap.Launch<Startup>(SourceType.Router);
        }
    }
}