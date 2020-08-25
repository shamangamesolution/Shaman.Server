using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Balancing;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Launchers.Common;
using Shaman.MM.Configuration;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.Routing.Balancing.Client;
using Shaman.Routing.Balancing.MM.Configuration;
using Shaman.Routing.Balancing.MM.Providers;
using Shaman.Routing.Common.Actualization;
using Shaman.Routing.Common.MM;
using Shaman.ServiceBootstrap.Logging;

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
            ConfigureSettings<MmApplicationConfig>(services);
            
            //install deps specific to launcher
            services.AddSingleton<IBundleInfoProviderConfig, BundleInfoProviderConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new BundleInfoProviderConfig(config.RouterUrl, config.PublicDomainNameOrAddress,config.ListenPorts, ServerRole.MatchMaker);
            });
            services.AddSingleton(provider => new RouterConfig(provider.GetService<IApplicationConfig>().RouterUrl));
            services.AddSingleton<IRouterClient, RouterClient>();
            services.AddSingleton<IRouterServerInfoProviderConfig, RouterServerInfoProviderConfig>(provider =>
            {
                var config = (MmApplicationConfig)provider.GetService<IApplicationConfig>();
                return new RouterServerInfoProviderConfig(config.ServerInfoListUpdateIntervalMs, config.ServerUnregisterTimeoutMs, config.GetIdentity());
            });
            services.AddSingleton<IMatchMakerServerInfoProvider, MatchMakerServerInfoProvider>();
            services.AddSingleton<IRoomApiProvider, DefaultRoomApiProvider>();
            services.AddSingleton<IServerActualizer, RouterServerActualizer>();
            services.AddSingleton<IBundleInfoProvider, BundleInfoProvider>();
            
            //metrics
            ConfigureMetrics<IMmMetrics, MmMetrics>(services);
        }
        

        
    }
}