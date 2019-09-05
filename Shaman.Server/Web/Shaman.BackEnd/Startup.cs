using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shaman.BackEnd.Caching;
using Shaman.BackEnd.Config;
using Shaman.BackEnd.Data.Containers;
using Shaman.BackEnd.Data.PlayerStorage;
using Shaman.BackEnd.Data.Repositories;
using Shaman.BackEnd.Data.Repositories.Interfaces;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Shared.Caching;
using LogLevel = Shaman.Common.Utils.Logging.LogLevel;

namespace Shaman.BackEnd
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
            services.AddSingleton<IShamanLogger, ConsoleLogger>(l => new ConsoleLogger("BE", LogLevel.Error | LogLevel.Info));

            var sp = services.BuildServiceProvider();
            var logger = sp.GetService<IShamanLogger>();
            
            logger.Initialize(SourceType.BackEnd, Configuration["ServerVersion"]);

            services.AddTransient<ISerializerFactory, SerializerFactory>();
            services.AddTransient<IPlayerStorage, PlayerStorage>();
            services.AddSingleton<IStorageContainerUpdater, BackendStorageUpdater>();
            services.AddSingleton<IStorageContainer, BackendStorageContainer>();
            services.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();
                       
            services.AddTransient<IStorageRepository, StorageRepository>(s => 
                new StorageRepository(Configuration["DbServerStatic"], Configuration["DbNameStatic"],Configuration["DbUserStatic"],Configuration["DbPasswordStatic"], logger));
            
            services.AddTransient<IParametersRepository, ParametersRepository>(s => 
                new ParametersRepository(Configuration["DbServerTemp"], Configuration["DbNameTemp"],Configuration["DbUserTemp"],Configuration["DbPasswordTemp"], logger));
            
            services.AddTransient<ITempRepository, TempRepository>(s => 
                new TempRepository(Configuration["DbServerTemp"], Configuration["DbNameTemp"],Configuration["DbUserTemp"],Configuration["DbPasswordTemp"], logger));
                            
            
            sp = services.BuildServiceProvider();
            
            services.AddTransient<IPlayerRepository, PlayerRepository>(s => 
                new PlayerRepository(Configuration["DbServer"], Configuration["DbName"],Configuration["DbUser"],Configuration["DbPassword"], logger, sp.GetService<ITempRepository>()));            

            services.AddSingleton<ICacher, RedisCacher>();            

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