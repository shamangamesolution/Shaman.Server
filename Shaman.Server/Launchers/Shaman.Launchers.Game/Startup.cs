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
            //install common deps
            base.ConfigureServices(services);

            ConfigureSettings<ApplicationConfig>(services);
            
            //deps specific to this launcher
            services.AddSingleton<IRoomControllerFactory, DefaultRoomControllerFactory>();
            services.AddSingleton<IRoomStateUpdater, RoomStateUpdater>();
            services.AddSingleton<IDefaultBundleInfoConfig, DefaultBundleInfoConfig>(c =>
                new DefaultBundleInfoConfig(Configuration["LauncherSettings:BundleUri"],
                    Convert.ToBoolean(Configuration["LauncherSettings:OverwriteDownloadedBundle"])));
            services.AddSingleton<IBundleInfoProvider, DefaultBundleInfoProvider>();
            services.AddSingleton<IServerActualizer, GameToMmServerActualizer>();
            services.AddSingleton<IMatchMakerInfoProvider, MatchMakerInfoProvider>(c => new MatchMakerInfoProvider(Configuration["LauncherSettings:MatchMakerUrl"]));
            services.AddSingleton<IBundleLoader, BundleLoader>();
            services.AddSingleton<IBundleSettingsProvider, BundleSettingsFromBundleLoaderProvider>();
            services.AddSingleton<IBundleConfig, BundleConfig>();
            ConfigureMetrics<IGameMetrics, GameMetrics>(services);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IServerActualizer serverActualizer, IShamanLogger logger)
        {
            base.ConfigureGame(app, env, server, serverActualizer, logger);
        }

    }
}