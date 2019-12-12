using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Serilog.Events;
using Shaman.Common.Metrics;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Configuration;
using Shaman.Game.Contract;
using Shaman.Game.Metrics;
using Shaman.Game.Providers;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.LiteNetLibAdapter;
using Shaman.ServerSharedUtilities;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.ServerSharedUtilities.Logging;
using LogLevel = Shaman.Common.Utils.Logging.LogLevel;

namespace Shaman.Game
{
    public class Startup
    {
        private ServiceCollection _serviceCollection;

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

            services.Configure<GameApplicationConfig>(Configuration);
            
            ConfigureMetrics(services);

            //get ports from Config
            var ports = Configuration["Ports"].Split(',').Select(s => Convert.ToUInt16(s)).ToList();

            services.AddSingleton<IShamanLogger, SerilogLogger>(f=>
            {
                var logger = new SerilogLogger(f.GetService<ILogger<SerilogLogger>>());
                ConfigureLogger(logger);
                return logger;
            });

            services.AddSingleton<IApplicationConfig>(c => 
                new GameApplicationConfig(
                    Configuration["Name"],
                    Configuration["Region"],
                    Configuration["PublicDomainNameOrAddress"], 
                    ports, 
                    Configuration["RouterUrl"], 
                    Configuration["MatchMakerUrl"],
                    Convert.ToUInt16(Configuration["BindToPortHttp"]),
                    Convert.ToInt32(Configuration["DestroyEmptyRoomOnMs"]), 
                    Convert.ToInt32(Configuration["ActualizationTimeoutMs"]),
                    Convert.ToInt32(Configuration["BackendListFromRouterIntervalMs"]),
                    Convert.ToBoolean(Configuration["AuthOn"]),
                    Configuration["Secret"],
                    Convert.ToInt32(Configuration["SocketTickTimeMs"]),
                    Convert.ToInt32(Configuration["ReceiveTickTimeMs"]),
                    Convert.ToInt32(Configuration["SendTickTimeMs"])
                ));
            services.AddSingleton<IPacketSenderConfig>(c => c.GetRequiredService<IApplicationConfig>()); 

            services.AddTransient<IPacketSender, PacketBatchSender>();
            services.AddScoped<IRoomPropertiesContainer, RoomPropertiesContainer>();            
            services.AddSingleton<ISerializer, BinarySerializer>();            
            services.AddSingleton<IRoomManager, RoomManager>();            
            //services.AddSingleton<ISocketFactory, HazelSockFactory>();
            services.AddSingleton<ISocketFactory, LiteNetSockFactory>();            

            services.AddTransient<ITaskSchedulerFactory, TaskSchedulerFactory>();            
            services.AddSingleton<IRequestSender, HttpSender>();            
            services.AddSingleton<IApplication, GameApplication>();
            services.AddSingleton<IBackendProvider, BackendProvider>();
            
//            services.AddSingleton<IStorageContainerUpdater, GameServerStorageUpdater>();
//            services.AddSingleton<IStorageContainer, GameServerStorageContainer>();
            services.AddSingleton<IGameServerInfoProvider, GameServerInfoProvider>();
            services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
            services.AddSingleton<IShamanComponents, ShamanComponents>();
            services.AddSingleton<IGameModeControllerFactory, GameModeControllerFactory>();
            services.AddSingleton<IBundleInfoProvider, BundleInfoProvider>();
            services.AddSingleton<IServerActualizer, ServerActualizer>();

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IGameServerInfoProvider serverInfoProvider, IGameModeControllerFactory controllerFactory/* init bundle */)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            server.Start();
            serverInfoProvider.Start();
        }

        private void ConfigureLogger(IShamanLogger logger)
        {
            logger.Initialize(SourceType.GameServer, Configuration["ServerVersion"],
                $"{Configuration["PublicDomainNameOrAddress"]}:{Configuration["BindToPortHttp"]}[{Configuration["Ports"]}]");
            var serilogLevel = Enum.Parse<LogEventLevel>(Configuration["Serilog:MinimumLevel"]);
            switch (serilogLevel)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                    logger.SetLogLevel(LogLevel.Debug | LogLevel.Error | LogLevel.Info);
                    break;
                case LogEventLevel.Information:
                    logger.SetLogLevel(LogLevel.Error | LogLevel.Info);
                    break;
                case LogEventLevel.Warning:
                case LogEventLevel.Error:
                    logger.SetLogLevel(LogLevel.Error);
                    break;
                case LogEventLevel.Fatal:
                    logger.SetLogLevel(LogLevel.Error);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}