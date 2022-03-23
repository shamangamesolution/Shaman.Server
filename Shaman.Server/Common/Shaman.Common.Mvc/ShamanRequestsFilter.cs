using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Bro.BackEnd.Mvc;

public class ShamanRequestsFilter : IAsyncActionFilter
{
    private class BlankResponse : HttpResponseBase
    {
        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
        }
    }

    private readonly IShamanLogger _logger;

    public ShamanRequestsFilter(IShamanLogger logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var model = context.ActionArguments.Values.FirstOrDefault();
        if (model is not HttpRequestBase)
        {
            await next();
            return;
        }

        try
        {
            await next();
        }
        catch (Exception ex)
        {
            BadResult(context, string.Empty);
            _logger.Error($"{context.HttpContext.Request.Path} error: {ex}");
        }
    }

    private static void BadResult(ActionExecutingContext context, string message)
    {
        var blankResponse = new BlankResponse();
        blankResponse.SetError(message);
        context.Result = new ShamanResult(blankResponse);
    }
}