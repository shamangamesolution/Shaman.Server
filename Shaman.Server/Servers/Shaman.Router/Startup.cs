using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Config;
using Shaman.Router.Data.Providers;
using Shaman.Router.Data.Repositories;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Metrics;
using Shaman.Serialization;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Router
{
    public class Startup
    {
        private readonly ILogger logger;
        
        public Startup(IHostingEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            this.logger = logger;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<RouterConfiguration>(Configuration);
            //services.AddSingleton<IShamanLogger, ConsoleLogger>(l => new ConsoleLogger("R", LogLevel.Error | LogLevel.Info));
            
            services.AddSingleton<IShamanLogger, SerilogLogger>();
            services.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();
            services.AddSingleton<ISerializer, BinarySerializer>();
            services.AddSingleton<IRouterServerInfoProvider, RouterServerInfoProvider>();

            var staticRoutesSection = Configuration.GetSection("StaticRoutes");
            if (staticRoutesSection.Exists())
            {
                var staticRoutes = new StaticConfigurationRepository.StaticRoutes();
                staticRoutesSection.Bind(staticRoutes);
                services.AddSingleton<IConfigurationRepository>(new StaticConfigurationRepository(staticRoutes));
            }
            else
            {
                services.AddTransient<IConfigurationRepository, ConfigurationRepository>();
                services.AddScoped<IRouterSqlDalProvider, RouterSqlDalProvider>();
            }
            
            ConfigureMetrics(services);

            services.AddMvc(opt => opt.EnableEndpointRouting = false);
        }
        private static IPAddress GetDnsIpAddress()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            return ipHostInfo.AddressList.Any() ? ipHostInfo.AddressList[0] : IPAddress.Loopback;
        }

        private void ConfigureMetrics(IServiceCollection services)
        {
            if (IsMetricsEnabled())
            {
                var metrisPAthPrefix = Configuration["MetricsPathPrefix"].Split(".").Concat(new []
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
        

        
        private bool IsMetricsEnabled()
        {
            return !string.IsNullOrEmpty(Configuration["GraphiteUrl"]);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IShamanLogger logger, IRouterServerInfoProvider serverInfoProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            if (IsMetricsEnabled())
                app.UseMiddleware<RequestMetricsMiddleWare>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            serverInfoProvider.Start();
            
            logger.Error($"Initial server list: {JsonConvert.SerializeObject(serverInfoProvider.GetAllServers(), Formatting.Indented)}");
            logger.Error($"Initial bundles list: {JsonConvert.SerializeObject(serverInfoProvider.GetAllBundles(), Formatting.Indented)}");
        }
    }
}