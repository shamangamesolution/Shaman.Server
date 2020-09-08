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
using Shaman.Contract.Routing.MM;
using Shaman.Launchers.Common;
using Shaman.Launchers.Common.Balancing;
using Shaman.Launchers.Common.MM;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using BalancingBundleInfoProviderConfig = Shaman.Bundling.Balancing.BalancingBundleInfoProviderConfig;
using IBalancingBundleInfoProviderConfig = Shaman.Bundling.Balancing.IBalancingBundleInfoProviderConfig;
using RouterBundleInfoProvider = Shaman.Bundling.Balancing.RouterBundleInfoProvider;

namespace Shaman.Launchers.MM.Balancing
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
            //install common deps
            base.ConfigureServices(services);
            
            //settings
            ConfigureSettings<ApplicationConfig>(services);
            
            //install deps specific to launcher
            services.AddSingleton<IBalancingBundleInfoProviderConfig, BalancingBundleInfoProviderConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new BalancingBundleInfoProviderConfig(Configuration["LauncherSettings:RouterUrl"], config.PublicDomainNameOrAddress,config.ListenPorts, config.ServerRole);
            });
            services.AddSingleton<IRoutingConfig, RoutingConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new RoutingConfig(Configuration["LauncherSettings:RouterUrl"],config.GetIdentity(), config.ServerName, config.Region, config.BindToPortHttp, 0);
            });
            services.AddSingleton<IRouterClient, RouterClient>();
            services.AddSingleton<IRouterServerInfoProviderConfig, RouterServerInfoProviderConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new RouterServerInfoProviderConfig(
                    Convert.ToInt32(Configuration["LauncherSettings:ServerInfoListUpdateIntervalMs"]),
                    Convert.ToInt32(Configuration["LauncherSettings:ServerUnregisterTimeoutMs"]),
                    config.GetIdentity());
            });
            services.AddSingleton<IPeerCountProvider, PeerCountProvider>();
            services.AddSingleton<IMatchMakerServerInfoProvider, MatchMakerServerInfoProvider>();
            services.AddSingleton<IRoomApiProvider, DefaultRoomApiProvider>();
            services.AddSingleton<IServerActualizer, RouterServerActualizer>();
            services.AddSingleton<IBundleInfoProvider, RouterBundleInfoProvider>();
            services.AddSingleton<IBundleLoader, BundleLoader>();
            //gets bundle settings from directory where bundle files are located
            services.AddSingleton<IBundleSettingsProvider, BundleSettingsFromBundleLoaderProvider>();
            //get particular bundle settings
            services.AddSingleton<IBundleConfig, BundleConfig>();
            //metrics
            ConfigureMetrics<IMmMetrics, MmMetrics>(services);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IShamanLogger logger, IMatchMaker matchMaker, IServerActualizer serverActualizer, IMatchMakerServerInfoProvider serverInfoProvider, IBundleLoader bundleLoader)
        {
            serverActualizer.Start(Convert.ToInt32(Configuration["LauncherSettings:ActualizationIntervalMs"]));
            
            base.ConfigureMm(app, env, server, logger, matchMaker, serverInfoProvider, bundleLoader);
        }
        
    }
}