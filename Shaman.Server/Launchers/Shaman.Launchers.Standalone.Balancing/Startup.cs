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
using Shaman.Contract.Routing.Actualization;
using Shaman.Contract.Routing.Balancing;
using Shaman.Contract.Routing.Meta;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.Launchers.Common;
using Shaman.Launchers.Common.Balancing;
using Shaman.Launchers.Common.Game;

namespace Shaman.Launchers.Standalone.Balancing
{
    public class Startup : GameStartup<DefaultRoomControllerFactory>
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
            services.AddSingleton<IRoomStateUpdater, FakeRoomStateUpdater>();

            ConfigureSettings<ApplicationConfig>(services);

            
            services.AddSingleton<IBalancingBundleInfoProviderConfig, BalancingBundleInfoProviderConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new BalancingBundleInfoProviderConfig(
                    Convert.ToBoolean(Configuration["LauncherSettings:OverwriteDownloadedBundle"]),
                    Configuration["LauncherSettings:RouterUrl"],
                    config.PublicDomainNameOrAddress, 
                    config.ListenPorts, 
                    config.ServerRole);
            });
            services.AddSingleton<IBundleInfoProvider, RouterBundleInfoProvider>();
            services.AddSingleton<IBundleLoader, BundleLoader>();

            services.AddSingleton<IServerActualizer, RouterServerActualizer>();
            services.AddSingleton<IServerStateHolder, ServerStateHolder>();
            services.AddSingleton<IServerStateProvider, ServerStateProvider>();
            services.AddSingleton<IRoutingConfig, RoutingConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new RoutingConfig(Configuration["LauncherSettings:RouterUrl"],config.GetIdentity(), config.ServerName, config.Region, config.BindToPortHttp, 0);
            });
            services.AddSingleton<IRouterClient, RouterClient>();
            //gets bundle settings from directory where bundle files are located
            services.AddSingleton<IBundleSettingsProvider, BundleSettingsFromBundleLoaderProvider>();
            //get particular bundle settings
            services.AddSingleton<IBundleConfig, BundleConfig>();
            ConfigureMetrics<IGameMetrics, GameMetrics>(services);
            //meta provider
            services.AddSingleton<IServerIdentityProvider, DefaultServerIdentityProvider>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new DefaultServerIdentityProvider(config.GetIdentity());
            });
            services.AddSingleton<IMetaProvider, RouterMetaProvider>();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IServerActualizer serverActualizer, IShamanLogger logger, IBundleLoader bundleLoader,
            IShamanComponents shamanComponents, IBundledRoomControllerFactory roomControllerFactory,
            IServerStateProvider serverStateProvider, IServerStateHolder serverStateHolder)
        {
            //todo extract in one place
            serverStateHolder.Update(serverStateProvider.GetState().Result);
            serverActualizer.Start(Convert.ToInt32(Configuration["ServerSettings:ActualizationIntervalMs"]));
            var gameBundle = bundleLoader.LoadTypeFromBundle<IGameBundle>();
            ConfigureGame(app, env, server, logger, gameBundle, roomControllerFactory, shamanComponents);
        }
    }
}