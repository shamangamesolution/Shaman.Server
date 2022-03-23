using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Shaman.ServiceBootstrap;

public static class WebApplicationExtensions
{
    public static async Task RunAsync(this Task<WebApplication> webApplication)
    {
        await (await webApplication).RunAsync();
    }
}