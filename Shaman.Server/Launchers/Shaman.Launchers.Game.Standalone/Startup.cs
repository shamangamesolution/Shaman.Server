using System;
using System.Diagnostics;
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
using Shaman.Game;
using Shaman.Game.Api;
using Shaman.Game.Configuration;
using Shaman.Game.Metrics;
using Shaman.Game.Providers;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.Launchers.Common;
using Shaman.LiteNetLibAdapter;
using Shaman.Routing.Common.Actualization;
using Shaman.Serialization;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Launchers.Game.Standalone
{
    public class Startup : GameStartup
    {

        public Startup(IConfiguration configuration) : base(configuration)
        {
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            
            services.AddSingleton<IRoomControllerFactory, StandaloneModeRoomControllerFactory>();
            services.AddSingleton<IApplicationConfig>(c => StandaloneServerLauncher.Config);
            services.AddSingleton<IGameMetrics, GameMetricsStub>();
            services.AddSingleton<IRoomStateUpdater, RoomStateUpdaterStub>();
            services.AddSingleton<IBundleInfoProvider, DefaultBundleInfoProvider>();
            services.AddSingleton<IServerActualizer, DefaultServerActualizer>();
        }
    }
}