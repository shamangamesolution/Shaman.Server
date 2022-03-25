using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Bro.BackEnd.Mvc;

public class ShamanResult : IActionResult
{
    private readonly HttpResponseBase _response;

    public ShamanResult(HttpResponseBase response)
    {
        _response = response;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        var serializer = serviceProvider.GetRequiredService<ISerializer>();
        var data = serializer.Serialize(_response);
        var executor =
            context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<FileContentResult>>();
        await executor.ExecuteAsync(context, new FileContentResult(data, "application/octet-stream"));
    }

    public static implicit operator ShamanResult(HttpResponseBase response)
    {
        return new ShamanResult(response);
    }
}