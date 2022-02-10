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
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.Launchers.Common;
using Shaman.Launchers.Common.Game;

namespace Shaman.Launchers.Game.Standalone
{
    public class Startup :
        //default room controller factory - it gets bundle from bundle loader
        GameStartup<DefaultRoomControllerFactory>
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
            
            //update room state on MM
            services.AddSingleton<IRoomStateUpdater, FakeRoomStateUpdater>();
            //used for configuration of bundle related services
            services.AddSingleton<IDefaultBundleInfoConfig, DefaultBundleInfoConfig>(c =>
                new DefaultBundleInfoConfig(Configuration["LauncherSettings:BundleUri"],
                    Convert.ToBoolean(Configuration["LauncherSettings:OverwriteDownloadedBundle"]),Configuration["CommonSettings:ServerRole"]));
            //gets information about bundle - its location
            services.AddSingleton<IBundleInfoProvider, DefaultBundleInfoProvider>();
            //gets information about mm - where to send stats
            //load bundle based on info from IBundleInfoProvider
            services.AddSingleton<IBundleLoader, BundleLoader>();
            //gets bundle settings from directory where bundle files are located
            services.AddSingleton<IBundleSettingsProvider, BundleSettingsFromBundleLoaderProvider>();
            //get particular bundle settings
            services.AddSingleton<IBundleConfig, BundleConfig>();
            //metrics
            ConfigureMetrics<IGameMetrics, GameMetrics>(services);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IShamanLogger logger, IBundleLoader bundleLoader, IShamanComponents shamanComponents, IBundledRoomControllerFactory roomControllerFactory)
        {
            var gameBundle = bundleLoader.LoadTypeFromBundle<IGameBundle>();
            ConfigureGame(app, env, server, logger, gameBundle, roomControllerFactory, shamanComponents);
        }
    }
}