using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
using Shaman.Game.Contract;
using Shaman.LiteNetLibAdapter;
using Shaman.MM.Configuration;
using Shaman.MM.MatchMaking;
using Shaman.MM.Players;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.ServerSharedUtilities.Logging;
using Shaman.MM.Contract;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.MM.Rooms;
using Shaman.ServerSharedUtilities;
using Shaman.ServerSharedUtilities.Bunlding;
using GameProject = Shaman.Messages.General.Entity.GameProject;

namespace Shaman.MM
{
    public class Startup
    {
        private static ITaskScheduler _globalTaskScheduler;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        
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
            
            ConfigureMetrics(services);
            
            services.Configure<MmApplicationConfig>(Configuration);
            var ports = Configuration["Ports"].Split(',').Select(s => Convert.ToUInt16(s)).ToList();
            //services.AddSingleton<IShamanLogger, ConsoleLogger>();//(l => new SerilogLogger(logLevel:LogLevel.Error | LogLevel.Info));            

            services.AddSingleton<IShamanLogger, SerilogLogger>();//(l => new SerilogLogger(logLevel:LogLevel.Error | LogLevel.Info));            
            services.AddSingleton<IApplicationConfig>(c => 
                new MmApplicationConfig(
                    Configuration["Region"],
                    Configuration["PublicDomainNameOrAddress"],
                    ports, 
                    Configuration["RouterUrl"],
                    Convert.ToInt32(Configuration["ServerInactivityTimeoutMs"]),
                    Convert.ToInt32(Configuration["ServerUnregisterTimeoutMs"]),
                    (GameProject)Convert.ToInt32(Configuration["GameProject"]), 
                    Configuration["name"], 
                    Convert.ToUInt16(Configuration["BindToPortHttp"]),
                    Convert.ToInt32(Configuration["SocketTickTimeMs"]),
                    Convert.ToInt32(Configuration["ReceiveTickTimeMs"]),
                    Convert.ToInt32(Configuration["SendTickTimeMs"]),
                    Convert.ToInt32(Configuration["BackendListFromRouterIntervalMs"]),
                    Convert.ToInt32(Configuration["ActualizeMatchmakerIntervalMs"]),
                    Convert.ToBoolean(Configuration["AuthOn"]), 
                    Configuration["Secret"],
                    serverInfoListUpdateIntervalMs: Convert.ToInt32(Configuration["ServerInfoListUpdateIntervalMs"])
                ));    
            
            services.AddSingleton<IPacketSenderConfig>(c => c.GetRequiredService<IApplicationConfig>()); 
            services.AddSingleton<IMatchMakerServerInfoProvider, MatchMakerServerInfoProvider>();

            services.AddSingleton<IPacketSender, PacketBatchSender>();
            services.AddSingleton<ISerializer, BinarySerializer>();            
            //services.AddSingleton<ISocketFactory, HazelSockFactory>();
            services.AddSingleton<ISocketFactory, LiteNetSockFactory>();            

            services.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();            
            services.AddSingleton<IRequestSender, HttpSender>();            
            services.AddSingleton<IMatchMaker, MatchMaker>();    
            services.AddSingleton<IApplication, MmApplication>();
            services.AddSingleton<IBackendProvider, BackendProvider>();
            services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
            services.AddSingleton<IMatchMakingGroupsManager, MatchMakingGroupManager>();
            services.AddSingleton<IPlayersManager, PlayersManager>();
            services.AddSingleton<IRoomManager, RoomManager>();
            services.AddSingleton<IServerActualizer, ServerActualizer>();
            services.AddSingleton<IBundleInfoProvider, BundleInfoProvider>();
            services.AddSingleton<IRoomPropertiesProvider, RoomPropertiesProvider>();
        }
        
        private void ConfigureMetrics(IServiceCollection services)
        {
            var metricsSettings = new MetricsSettings();
            Configuration.GetSection("Metrics").Bind(metricsSettings);
            var metricsAgent = new MetricsAgent(metricsSettings);
            services.AddSingleton<IMetricsAgent>(metricsAgent);
            services.AddSingleton<IMmMetrics, MmMetrics>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory,
            IMatchMaker matchMaker,
            IBundleInfoProvider bundleInfoProvider, IServerActualizer serverActualizer, IMatchMakerServerInfoProvider serverInfoProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            
            //setup logging based on Serilog params (is not very good, but let it be so)
            logger.Initialize(SourceType.MatchMaker, Configuration["ServerVersion"], $"{Configuration["PublicDomainNameOrAddress"]}:{Configuration["BindToPortHttp"]}[{Configuration["Ports"]}]");
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
            
            var bundleUri = bundleInfoProvider.GetBundleUri().Result;
            var resolver = BundleHelper.LoadTypeFromBundle<IMmResolver>(bundleUri, Convert.ToBoolean(Configuration["OverwriteDownloadedBundle"]));
            RoomPropertiesProvider.RoomPropertiesProviderImplementation = resolver.GetRoomPropertiesProvider();
            resolver.Configure(matchMaker);
            
            serverInfoProvider.Start();

            _globalTaskScheduler = taskSchedulerFactory.GetTaskScheduler();
            server.Start();
        }
        
    }
}