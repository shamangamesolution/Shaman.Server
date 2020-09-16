using Shaman.Contract.Routing;
using Shaman.ServiceBootstrap;

namespace Shaman.Router
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            Bootstrap.Launch<Startup>();
        }
    }
}