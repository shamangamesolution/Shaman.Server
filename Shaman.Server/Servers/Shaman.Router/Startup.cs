﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Router.Config;
using Shaman.Router.Data.Providers;
using Shaman.Router.Data.Repositories;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.ServerSharedUtilities.Logging;
using LogLevel = Shaman.Common.Utils.Logging.LogLevel;

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
            
            services.AddSingleton<IShamanLogger, ConsoleLogger>();
            services.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();
            services.AddSingleton<ISerializer, BinarySerializer>();
            services.AddSingleton<IRouterServerInfoProvider, RouterServerInfoProvider>();
            
            services.AddTransient<IConfigurationRepository, ConfigurationRepository>();
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            logger.Initialize(SourceType.Router, Configuration["ServerVersion"]);
            var serilogLevel = Enum.Parse<LogEventLevel>(Configuration["Serilog:MinimumLevel"]);
            switch (serilogLevel)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                    logger.SetLogLevel(LogLevel.Debug | LogLevel.Error | LogLevel.Info);
                    break;
                case LogEventLevel.Information:
                    logger.SetLogLevel(LogLevel.Error | LogLevel.Info);
                    break;
                case LogEventLevel.Warning:
                case LogEventLevel.Error:
                    logger.SetLogLevel(LogLevel.Error);
                    break;
                case LogEventLevel.Fatal:
                    logger.SetLogLevel(LogLevel.Error);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            serverInfoProvider.Start();
            
        }
    }
}