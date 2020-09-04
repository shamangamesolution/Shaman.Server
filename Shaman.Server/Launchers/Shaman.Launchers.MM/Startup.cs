using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.Contract.Routing.MM;
using Shaman.Launchers.Common;
using Shaman.Launchers.Common.MM;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.MM.Rooms;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Launchers.MM
{
    public class Startup : MmStartup
    {
        public Startup(IConfiguration configuration)
            :base(configuration)
        {
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public override void ConfigureServices(IServiceCollection services)
        {
            //configure all services related to common Game server launchers 
            base.ConfigureServices(services);
            
            //settings
            ConfigureSettings<ApplicationConfig>(services);

            //object responsible for providing info about game servers which are ready to accepting players
            services.AddSingleton<IMatchMakerServerInfoProvider, DefaultMatchMakerServerInfoProvider>();
            //api for creating and updating rooms 
            services.AddSingleton<IRoomApiProvider, DefaultRoomApiProvider>();
            //used for configuration of bundle related services
            services.AddSingleton<IDefaultBundleInfoConfig, DefaultBundleInfoConfig>(c =>
                new DefaultBundleInfoConfig(Configuration["LauncherSettings:BundleUri"],
                    Convert.ToBoolean(Configuration["LauncherSettings:OverwriteDownloadedBundle"])));
            //gets information about bundle - its location
            services.AddSingleton<IBundleInfoProvider, DefaultBundleInfoProvider>();
            //load bundle based on info from IBundleInfoProvider
            services.AddSingleton<IBundleLoader, BundleLoader>();
            //gets bundle settings from directory where bundle files are located
            services.AddSingleton<IBundleSettingsProvider, BundleSettingsFromBundleLoaderProvider>();
            //get particular bundle settings
            services.AddSingleton<IBundleConfig, BundleConfig>();
            //metrics
            ConfigureMetrics<IMmMetrics, MmMetrics>(services);
        }
        

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IShamanLogger logger, IMatchMaker matchMaker, IMatchMakerServerInfoProvider serverInfoProvider, IBundleLoader bundleLoader)
        {
            base.ConfigureMm(app, env, server, logger, matchMaker, serverInfoProvider, bundleLoader);
        }
    }
}