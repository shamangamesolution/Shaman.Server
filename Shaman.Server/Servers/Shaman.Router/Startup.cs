using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shaman.Common.Mvc;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Config;
using Shaman.Router.Data.Providers;
using Shaman.Router.Data.Repositories;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Metrics;
using Shaman.Serialization;
using Shaman.ServiceBootstrap;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Router
{
    public class Startup : IShamanWebStartup
    {
        private static IPAddress GetDnsIpAddress()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            return ipHostInfo.AddressList.Any() ? ipHostInfo.AddressList[0] : IPAddress.Loopback;
        }

        private void ConfigureMetrics(IServiceCollection services, IConfiguration Configuration)
        {
            if (IsMetricsEnabled(Configuration))
            {
                var metrisPAthPrefix = Configuration["MetricsPathPrefix"].Split(".").Concat(new[]
                {
                    Configuration["ServerVersion"],
                    "Router",
                    IpV4Helper.Get20BitMaskAsString(GetDnsIpAddress())
                }).ToArray();
                services
                    .AddCollectingRequestMetricsToGraphite(
                        Configuration["GraphiteUrl"],
                        TimeSpan.FromSeconds(10),
                        metrisPAthPrefix);
            }
        }

        private bool IsMetricsEnabled(IConfiguration Configuration)
        {
            return !string.IsNullOrEmpty(Configuration["GraphiteUrl"]);
        }


        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RouterConfiguration>(configuration);
            //services.AddSingleton<IShamanLogger, ConsoleLogger>(l => new ConsoleLogger("R", LogLevel.Error | LogLevel.Info));

            services.AddSingleton<IShamanLogger, SerilogLogger>();
            services.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();
            services.AddSingleton<ISerializer, BinarySerializer>();
            services.AddSingleton<IRouterServerInfoProvider, RouterServerInfoProvider>();
            services.AddSingleton<IStatesManager, StatesManager>();

            var staticRoutesSection = configuration.GetSection("StaticRoutes");
            if (staticRoutesSection.Exists())
            {
                var staticRoutes = new StaticConfigurationRepository.StaticRoutes();
                staticRoutesSection.Bind(staticRoutes);
                services.AddSingleton<IConfigurationRepository>(new StaticConfigurationRepository(staticRoutes));
            }
            else
            {
                services.AddTransient<IConfigurationRepository, ConfigurationRepository>();
                services.AddTransient<IStateRepository, StateRepository>();
                services.AddSingleton<IRouterSqlDalProvider, RouterSqlDalProvider>();
            }


            ConfigureMetrics(services, configuration);
        }

        public void ConfigureApp(WebApplication webApplication)
        {
        }

        public void AddMvcOptions(MvcOptions options) => options.AddShamanMvc();

        public async Task Initialize(IServiceProvider services)
        {
            var logger = services.GetRequiredService<IShamanLogger>();
            using (var scope = services.CreateScope())
            {
                var routerDbInitializer = new RouterDbInitializer(services.GetRequiredService<IRouterSqlDalProvider>());
                await routerDbInitializer.Initialize();
                var serverInfoProvider = scope.ServiceProvider.GetRequiredService<IRouterServerInfoProvider>();
                serverInfoProvider.Start();

                logger.Error(
                    $"Initial server list: {JsonConvert.SerializeObject(serverInfoProvider.GetAllServers(), Formatting.Indented)}");
                logger.Error(
                    $"Initial bundles list: {JsonConvert.SerializeObject(serverInfoProvider.GetAllBundles(), Formatting.Indented)}");
                
                var statesProvider = scope.ServiceProvider.GetRequiredService<IStatesManager>();
                statesProvider.Start();
            }
        }

        public IEnumerable<Type> GetMiddleWares(IServiceProvider serviceProvider)
        {
            if (IsMetricsEnabled(serviceProvider.GetRequiredService<IConfiguration>()))
                yield return typeof(RequestMetricsMiddleWare);
        }
    }
}