using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Balancing;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Contract.Common.Logging;
using Shaman.Launchers.Common.MM;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.Routing.Balancing.Client;
using Shaman.Routing.Balancing.MM.Configuration;
using Shaman.Routing.Balancing.MM.Providers;
using Shaman.Routing.Common.Actualization;
using Shaman.Routing.Common.MM;

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
            services.AddSingleton(provider => new RouterConfig(Configuration["LauncherSettings:RouterUrl"]));
            services.AddSingleton<IRouterClient, RouterClient>();
            services.AddSingleton<IRouterServerInfoProviderConfig, RouterServerInfoProviderConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new RouterServerInfoProviderConfig(
                    Convert.ToInt32(Configuration["LauncherSettings:ServerInfoListUpdateIntervalMs"]),
                    Convert.ToInt32(Configuration["LauncherSettings:ServerUnregisterTimeoutMs"]),
                    config.GetIdentity());
            });
            services.AddSingleton<IMatchMakerServerInfoProvider, MatchMakerServerInfoProvider>();
            services.AddSingleton<IRoomApiProvider, DefaultRoomApiProvider>();
            services.AddSingleton<IServerActualizer, RouterServerActualizer>();
            services.AddSingleton<IBundleInfoProvider, BundleInfoProvider>();
            services.AddSingleton<IBundleLoader, BundleLoader>();

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