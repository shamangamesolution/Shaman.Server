using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Balancing;
using Shaman.Bundling.Common;
using Shaman.Common.Metrics;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Contract.Bundle;
using Shaman.Game;
using Shaman.Game.Configuration;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.Launchers.Common;
using Shaman.Routing.Balancing.Client;
using Shaman.Routing.Common.Actualization;

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

            services.Configure<GameApplicationConfig>(Configuration);
            var ports = Configuration["Ports"].Split(',').Select(s => Convert.ToUInt16(s)).ToList();
            services.AddSingleton<IApplicationConfig>(c => 
                new GameApplicationConfig(
                    Configuration["Name"],
                    Configuration["Region"],
                    Configuration["PublicDomainNameOrAddress"], 
                    ports, 
                    Configuration["RouterUrl"], 
                    Convert.ToUInt16(Configuration["BindToPortHttp"]),
                    Convert.ToBoolean(Configuration["AuthOn"]),
                    Configuration["Secret"],
                    Convert.ToInt32(Configuration["SocketTickTimeMs"]),
                    Convert.ToInt32(Configuration["ReceiveTickTimeMs"]),
                    Convert.ToInt32(Configuration["SendTickTimeMs"]),
                    actualizationIntervalMs: Convert.ToInt32(Configuration["ActualizationIntervalMs"])
                )
                {
                    OverwriteDownloadedBundle = Convert.ToBoolean(Configuration["OverwriteDownloadedBundle"])
                });
            
            services.AddSingleton<IBundleInfoProviderConfig, BundleInfoProviderConfig>(provider =>
            {
                var config = provider.GetService<IApplicationConfig>();
                return new BundleInfoProviderConfig(config.GetRouterUrl(), config.GetPublicName(),config.GetListenPorts(), ServerRole.GameServer);
            });
            services.AddSingleton<IBundleInfoProvider, BundleInfoProvider>();
            services.AddSingleton<IServerActualizer, RouterServerActualizer>();
            services.AddSingleton(provider => new RouterConfig(provider.GetService<IApplicationConfig>().GetRouterUrl()));
            services.AddSingleton<IRouterClient, RouterClient>();

            ConfigureMetrics(services);
        }
        
        private void ConfigureMetrics(IServiceCollection services)
        {
            var metricsSettings = new MetricsSettings();
            Configuration.GetSection("Metrics").Bind(metricsSettings);
            var metricsAgent = new MetricsAgent(metricsSettings);
            services.AddSingleton<IMetricsAgent>(metricsAgent);
            services.AddSingleton<IGameMetrics, GameMetrics>();
        }


    }
}