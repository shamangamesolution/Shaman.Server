using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Shaman.Common.Metrics;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Game.Api;
using Shaman.Game.Configuration;
using Shaman.Game.Metrics;
using Shaman.Game.Providers;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.LiteNetLibAdapter;
using Shaman.Serialization;
using Shaman.ServerSharedUtilities;
using Shaman.ServerSharedUtilities.Bundling;
using Shaman.ServerSharedUtilities.Logging;

namespace Shaman.Game
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMvc().AddJsonOptions(o =>
            {
                o.SerializerSettings.ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };
            });

            if (StandaloneServerLauncher.IsStandaloneMode)
            {
                services.AddSingleton<IRoomControllerFactory, StandaloneModeRoomControllerFactory>();
                services.AddSingleton<IApplicationConfig>(c => StandaloneServerLauncher.Config);
                services.AddSingleton<IGameMetrics, GameMetricsStub>();
                services.AddSingleton<IRoomStateUpdater, RoomStateUpdaterStub>();
            }
            else
            {
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
                        Configuration["MatchMakerUrl"],
                        Convert.ToUInt16(Configuration["BindToPortHttp"]),
                        Convert.ToInt32(Configuration["ActualizationTimeoutMs"]),
                        Convert.ToInt32(Configuration["BackendListFromRouterIntervalMs"]),
                        Convert.ToBoolean(Configuration["AuthOn"]),
                        Configuration["Secret"],
                        Convert.ToInt32(Configuration["SocketTickTimeMs"]),
                        Convert.ToInt32(Configuration["ReceiveTickTimeMs"]),
                        Convert.ToInt32(Configuration["SendTickTimeMs"])
                    )
                    {
                        OverwriteDownloadedBundle = Convert.ToBoolean(Configuration["OverwriteDownloadedBundle"])
                    });
                ConfigureMetrics(services);
            }

            services.AddSingleton<IShamanLogger, SerilogLogger>();

           
            services.AddSingleton<IPacketSenderConfig>(c => c.GetRequiredService<IApplicationConfig>()); 
            services.AddSingleton<IApplicationCoreConfig>(c => c.GetRequiredService<IApplicationConfig>()); 

            services.AddSingleton<IPacketSender, PacketBatchSender>();
            services.AddTransient<IShamanMessageSenderFactory, ShamanMessageSenderFactory>();

            services.AddScoped<IRoomPropertiesContainer, RoomPropertiesContainer>();            
            services.AddSingleton<ISerializer, BinarySerializer>();            
            services.AddSingleton<IRoomManager, RoomManager>();
            //services.AddSingleton<ISocketFactory, HazelSockFactory>();
            services.AddSingleton<ISocketFactory, LiteNetSockFactory>();            

            services.AddTransient<ITaskSchedulerFactory, TaskSchedulerFactory>();            
            services.AddSingleton<IRequestSender, HttpSender>();            
            services.AddSingleton<IApplication, GameApplication>();
            
            services.AddSingleton<IGameServerInfoProvider, GameServerInfoProvider>();
            services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
            services.AddSingleton<IShamanComponents, ShamanComponents>();
            services.AddSingleton<IBundleInfoProvider, BundleInfoProvider>();
            services.AddSingleton<IServerActualizer, ServerActualizer>();
            services.AddSingleton<IGameServerApi, GameServerApi>();
            
        }
        private void ConfigureMetrics(IServiceCollection services)
        {
            var metricsSettings = new MetricsSettings();
            Configuration.GetSection("Metrics").Bind(metricsSettings);
            var metricsAgent = new MetricsAgent(metricsSettings);
            services.AddSingleton<IMetricsAgent>(metricsAgent);
            services.AddSingleton<IGameMetrics, GameMetrics>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IGameServerInfoProvider serverInfoProvider, IRoomControllerFactory controllerFactory /* init bundle */,
            IGameServerApi gameServerApi, IShamanLogger logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                CheckProductionCompiledInRelease(logger);
            }

            app.UseMvc();

            server.Start();
            
            if (!StandaloneServerLauncher.IsStandaloneMode)
                serverInfoProvider.Start();
            else
                StandaloneServerLauncher.Api = gameServerApi;
        }

        [Conditional("DEBUG")]
        public void CheckProductionCompiledInRelease(IShamanLogger logger)
        {
            logger.Error("ATTENTION!!! Release Environment compiled in DEBUG mode!");
        }
    }
}