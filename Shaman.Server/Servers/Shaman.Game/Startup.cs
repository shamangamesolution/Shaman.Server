using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Serilog.Events;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Configuration;
using Shaman.Game.Data;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.GameModeControllers;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.HazelAdapter;
using Shaman.Messages.General.Entity.Storage;
using Shaman.ServerSharedUtilities.Backends;
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

            services.Configure<GameApplicationConfig>(Configuration);

            //get ports from Config
            var ports = Configuration["Ports"].Split(',').Select(s => Convert.ToUInt16(s)).ToArray();

            services.AddSingleton<IShamanLogger, SerilogLogger>();//(l => new SerilogLogger(logLevel:LogLevel.Error | LogLevel.Info));            
            //services.AddSingleton<IShamanLogger, ConsoleLogger>();//(l => new SerilogLogger(logLevel:LogLevel.Error | LogLevel.Info));            

            services.AddSingleton<IApplicationConfig, GameApplicationConfig>(c => 
                new GameApplicationConfig(
                    Configuration["PublicDomainNameOrAddress"], 
                    ports, 
                    Configuration["RouterUrl"], 
                    Configuration["MatchMakerUrl"],
                    Convert.ToUInt16(Configuration["BindToPortHttp"]),
                    Convert.ToInt32(Configuration["DestroyEmptyRoomOnMs"]), 
                    Convert.ToInt32(Configuration["ActualizationTimeoutMs"]),
                    Convert.ToInt32(Configuration["BackendListFromRouterIntervalMs"]),
                    Convert.ToBoolean(Configuration["AuthOn"]),
                    Convert.ToInt32(Configuration["SocketTickTimeMs"]),
                    Convert.ToInt32(Configuration["ReceiveTickTimeMs"]),
                    Convert.ToInt32(Configuration["SendTickTimeMs"])
                ));          
            
            services.AddTransient<IPacketSender, PacketBatchSender>();
            services.AddScoped<IRoomPropertiesContainer, RoomPropertiesContainer>();            
            services.AddTransient<IRoomManager, RoomManager>();            
            services.AddTransient<IGameModeControllerFactory, MsGameModeControllerFactory>();            
            services.AddTransient<IPacketSender, PacketBatchSender>();
            services.AddSingleton<ISerializerFactory, SerializerFactory>();            
            services.AddSingleton<ISocketFactory, HazelSockFactory>();            
            services.AddTransient<ITaskSchedulerFactory, TaskSchedulerFactory>();            
            services.AddSingleton<IRequestSender, HttpSender>();            
            services.AddSingleton<IApplication, GameApplication>();
            services.AddSingleton<IBackendProvider, BackendProvider>();
            services.AddSingleton<IStorageContainerUpdater, GameServerStorageUpdater>();
            services.AddSingleton<IStorageContainer, GameServerStorageContainer>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IStorageContainer storageContainer, IShamanLogger logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //setup logging based on Serilog params (is not very good, but let it be so)
            logger.Initialize(SourceType.GameServer, Configuration["ServerVersion"], $"{Configuration["PublicDomainNameOrAddress"]}:{Configuration["BindToPortHttp"]}[{Configuration["Ports"]}]");
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
            
            app.UseMvc();
            
            server.Start();
            
            storageContainer.Start(Configuration["ServerVersion"]);
        }
    }
}