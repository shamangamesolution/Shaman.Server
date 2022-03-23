using System.Threading.Tasks;
using Shaman.ServiceBootstrap;

namespace Shaman.Router
{
    public static class Program
    {
        internal static async Task Main(string[] args)
        {
            await Bootstrap.BuildWebApp<Startup>().RunAsync();
        }
    }
}