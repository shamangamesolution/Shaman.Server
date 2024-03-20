using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shaman.ServiceBootstrap;

public interface IShamanWebStartup
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void ConfigureApp(WebApplication webApplication);
    void AddMvcOptions(MvcOptions options);
    Task Initialize(IServiceProvider services);
    IEnumerable<Type> GetMiddleWares(IServiceProvider serviceProvider);
}