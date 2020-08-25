using Shaman.Common.Server.Messages;
using Shaman.ServiceBootstrap;

namespace Shaman.Router
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            Bootstrap.Launch<Startup>(ServerRole.Router);
        }
    }
}