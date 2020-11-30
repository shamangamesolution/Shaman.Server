using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Shaman.Bundling.Common;
using Shaman.Common.Http;
using Shaman.Common.Metrics;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.Actualization;
using Shaman.Contract.Routing.Meta;
using Shaman.Game;
using Shaman.Game.Api;
using Shaman.Game.Metrics;
using Shaman.Game.Providers;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.Launchers.Common;
using Shaman.Launchers.Common.Game;
using Shaman.Launchers.Game.Routing;
using Shaman.LiteNetLibAdapter;
using Shaman.Routing.Common;
using Shaman.Serialization;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Launchers.Game
{
    public class Startup : GameStartup
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
            
            //default room controller factory - it gets bundle from bundle loader
            services.AddSingleton<IRoomControllerFactory, DefaultRoomControllerFactory>();
            //update room state on MM
            services.AddSingleton<IRoomStateUpdater, RoomStateUpdater>();
            //used for configuration of bundle related services
            services.AddSingleton<IDefaultBundleInfoConfig, DefaultBundleInfoConfig>(c =>
                new DefaultBundleInfoConfig(Configuration["LauncherSettings:BundleUri"],
                    Convert.ToBoolean(Configuration["LauncherSettings:OverwriteDownloadedBundle"]),Configuration["CommonSettings:ServerRole"]));
            //gets information about bundle - its location
            services.AddSingleton<IBundleInfoProvider, DefaultBundleInfoProvider>();
            //actualize game server on mm
            services.AddSingleton<IServerActualizer, GameToMmServerActualizer>();
            //gets information about mm - where to send stats
            //in this type of launcher this info is in launcher config file
            services.AddSingleton<IMatchMakerInfoProvider, MatchMakerInfoProvider>(c => new MatchMakerInfoProvider(Configuration["LauncherSettings:MatchMakerUrl"]));
            //load bundle based on info from IBundleInfoProvider
            services.AddSingleton<IBundleLoader, BundleLoader>();
            //gets bundle settings from directory where bundle files are located
            services.AddSingleton<IBundleSettingsProvider, BundleSettingsFromBundleLoaderProvider>();
            //get particular bundle settings
            services.AddSingleton<IBundleConfig, BundleConfig>();
            //metrics
            ConfigureMetrics<IGameMetrics, GameMetrics>(services);
            //meta
            services.AddSingleton<IMetaProvider, BundleSettingsMetaProvider>();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IServerActualizer serverActualizer, IShamanLogger logger, IBundleLoader bundleLoader,
            IShamanComponents shamanComponents, IRoomControllerFactory roomControllerFactory)
        {
            //todo extract in one place
            serverActualizer.Start(Convert.ToInt32(Configuration["ServerSettings:ActualizationIntervalMs"]));

            bundleLoader.LoadBundle();
            var gameBundle = bundleLoader.LoadTypeFromBundle<IGameBundle>();
            gameBundle.OnInitialize(shamanComponents);
            var bundledRoomControllerFactory = gameBundle.GetRoomControllerFactory();
            if (bundledRoomControllerFactory == null)
            {
                throw new NullReferenceException("Game bundle returned null factory");
            }
            
            ((IBundleRoomControllerRegistry)roomControllerFactory).RegisterBundleRoomController(bundledRoomControllerFactory);            
            
            base.ConfigureGame(app, env, server, logger);
        }


    }
}