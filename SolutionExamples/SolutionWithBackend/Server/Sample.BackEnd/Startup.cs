using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.BackEnd.Caching;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.Containers;
using Sample.BackEnd.Data.PlayerStorage;
using Sample.BackEnd.Data.Repositories;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.Shared.Data.Storage;
using Serilog.Events;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.ServerSharedUtilities.Logging;
using LogLevel = Shaman.Common.Utils.Logging.LogLevel;

namespace Sample.BackEnd
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

            services.Configure<BackendConfiguration>(Configuration);
            
            //singletons
            services.AddSingleton<IShamanLogger, SerilogLogger>();
            services.AddSingleton<IStorageContainerUpdater, BackendStorageUpdater>();
            services.AddSingleton<IStorageContainer, BackendStorageContainer>();
            services.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();
            services.AddSingleton<ICacher, RedisCacher>();            
            
            //transients
            services.AddTransient<ISerializer, BinarySerializer>();
            services.AddTransient<IStorageRepository, StorageRepository>();
            services.AddTransient<IParametersRepository, ParametersRepository>();
            services.AddTransient<ITempRepository, TempRepository>();
            services.AddTransient<IShopRepository, ShopRepository>();
            services.AddTransient<IExternalAccountsRepository, ExternalAccountsRepository>();
            services.AddTransient<IPlayerRepository, PlayerRepository>();            

            //scoped
            services.AddScoped<IPlayerStorage, PlayerStorage>();
                       
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IStorageContainer container)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            container.Start(Configuration["ServerVersion"]);
        }
    }
}