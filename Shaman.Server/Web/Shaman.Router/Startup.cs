using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories;
using Shaman.Router.Data.Repositories.Interfaces;
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
            services.AddSingleton<IShamanLogger, ConsoleLogger>(l => new ConsoleLogger("R", LogLevel.Error | LogLevel.Info));
            services.AddSingleton<ISerializerFactory, SerializerFactory>();
            
            var sp = services.BuildServiceProvider();
            var logger = sp.GetService<IShamanLogger>();
            logger.Initialize(SourceType.Router, Configuration["ServerVersion"]);
            
            services.AddTransient<IConfigurationRepository, ConfigurationRepository>(s => new ConfigurationRepository(Configuration["DbServer"], Configuration["DbName"],Configuration["DbUser"],Configuration["DbPassword"], logger));
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
        }
    }
}