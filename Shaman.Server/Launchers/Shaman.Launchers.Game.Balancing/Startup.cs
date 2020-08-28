using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Balancing;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.Balancing;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.Launchers.Common.Game;
using Shaman.Routing.Balancing.Client;
using Shaman.Routing.Common;

namespace Shaman.Launchers.Game.Balancing
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
            
            //deps specific to this launcher
            services.AddSingleton<IRoomControllerFactory, DefaultRoomControllerFactory>();
            services.AddSingleton<IRoomStateUpdater, RoomStateUpdater>();

            ConfigureSettings<ApplicationConfig>(services);

            services.AddSingleton<IBalancingBundleInfoProviderConfig, BalancingBundleInfoProviderConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new BalancingBundleInfoProviderConfig(Configuration["LauncherSettings:RouterUrl"], config.PublicDomainNameOrAddress, config.ListenPorts, config.ServerRole);
            });
            services.AddSingleton<IBundleInfoProvider, RouterBundleInfoProvider>();
            services.AddSingleton<IBundleLoader, BundleLoader>();

            services.AddSingleton<IServerActualizer, RouterServerActualizer>();
            services.AddSingleton<IPeerCountProvider, PeerCountProvider>();
            services.AddSingleton<IRoutingConfig, RoutingConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new RoutingConfig(Configuration["LauncherSettings:RouterUrl"],config.GetIdentity(), config.ServerName, config.Region, config.BindToPortHttp, 0);
            });
            services.AddSingleton<IRouterClient, RouterClient>();

            ConfigureMetrics<IGameMetrics, GameMetrics>(services);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IServerActualizer serverActualizer, IShamanLogger logger)
        {
            base.ConfigureGame(app, env, server, serverActualizer, logger);
        }
    }
}