using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bro.BackEnd.Bootstrap;

public interface IShamanWebStartup
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void AddMvcOptions(MvcOptions options);
    Task Initialize(IServiceProvider services);
    IEnumerable<Type> GetMiddleWares(IServiceProvider serviceProvider);
}