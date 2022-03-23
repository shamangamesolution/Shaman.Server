using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Common.Http;
using Shaman.Serialization;

namespace Bro.BackEnd.Mvc;

public class ShamanModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        // currently we have only shaman-serialized requests
        return new ShamanModelBinder(context.Services.GetRequiredService<ISerializer>());
    }

    class ShamanModelBinder : IModelBinder
    {
        private readonly ISerializer _serializer;

        public ShamanModelBinder(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var data = await bindingContext.HttpContext.Request.GetRawBodyBytesAsync();
            var model = _serializer.Deserialize(data, bindingContext.ModelType);
            bindingContext.Result = ModelBindingResult.Success(model);
        }
    }
}