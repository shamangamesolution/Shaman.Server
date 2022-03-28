using Microsoft.AspNetCore.Mvc;

namespace Shaman.Common.Mvc;

public static class ShamanMvcExtensions
{
    public static void AddShamanMvc(this MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, new ShamanModelBinderProvider());
        options.Filters.Add<ShamanRequestsFilter>();
    }
}