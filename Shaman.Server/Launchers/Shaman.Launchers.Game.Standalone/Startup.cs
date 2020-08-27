using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Bundle;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.Launchers.Common.Game;
using Shaman.Routing.Common.Actualization;

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