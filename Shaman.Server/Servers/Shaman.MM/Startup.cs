using System;
using System.Collections.Generic;
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
using Shaman.HazelAdapter;
using Shaman.MM.Configuration;
using Shaman.MM.MatchMaking;
using Shaman.MM.Players;
using Shaman.MM.Servers;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.ServerSharedUtilities.Logging;
using Shaman.Messages;
using Shaman.Messages.General.Entity;

namespace Shaman.MM
{
    public class Startup
    {
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
            
            services.Configure<MmApplicationConfig>(Configuration);
            var ports = Configuration["Ports"].Split(',').Select(s => Convert.ToUInt16(s)).ToArray();
            //services.AddSingleton<IShamanLogger, ConsoleLogger>();//(l => new SerilogLogger(logLevel:LogLevel.Error | LogLevel.Info));            

            services.AddSingleton<IShamanLogger, SerilogLogger>();//(l => new SerilogLogger(logLevel:LogLevel.Error | LogLevel.Info));            
            services.AddSingleton<IApplicationConfig, MmApplicationConfig>(c => 
                new MmApplicationConfig(
                    Configuration["PublicDomainNameOrAddress"],
                    ports, 
                    Configuration["RouterUrl"],
                    Convert.ToInt32(Configuration["ServerInactivityTimeoutMs"]),
                    Convert.ToInt32(Configuration["ServerUnregisterTimeoutMs"]),
                    (GameProject)Convert.ToInt32(Configuration["GameProject"]), 
                    Configuration["name"], 
                    Configuration["Secret"],
                    Convert.ToInt32(Configuration["SocketTickTimeMs"]),
                    Convert.ToInt32(Configuration["ReceiveTickTimeMs"]),
                    Convert.ToInt32(Configuration["SendTickTimeMs"]),
                    Convert.ToInt32(Configuration["BackendListFromRouterIntervalMs"]),
                    Convert.ToInt32(Configuration["ActualizeMatchmakerIntervalMs"]),
                    Convert.ToBoolean(Configuration["AuthOn"])
                ));    
            services.AddTransient<IPacketSender, PacketBatchSender>();
            services.AddSingleton<ISerializerFactory, SerializerFactory>();            
            services.AddSingleton<ISocketFactory, HazelSockFactory>();            
            services.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();            
            services.AddSingleton<IRequestSender, HttpSender>();            
            services.AddSingleton<IRegisteredServerCollection, RegisteredServersCollection>();    
            services.AddSingleton<IPlayerCollection, PlayerCollection>();    
            services.AddSingleton<IMatchMaker, MatchMaker>();    
            services.AddSingleton<IApplication, MmApplication>();
            services.AddSingleton<IBackendProvider, BackendProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IShamanLogger logger, IPlayerCollection playerColelction, ITaskSchedulerFactory taskSchedulerFactory, IRegisteredServerCollection registeredServerCollection, IMatchMaker matchMaker)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
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

            ((MmApplication)server).SetMatchMakerProperties(new List<byte>{ PropertyCode.PlayerProperties.GameMode });
            
            matchMaker.AddMatchMakingGroup(4, 250, true, true, 5000, new Dictionary<byte, object> {{PropertyCode.RoomProperties.GameMode, (byte)GameMode.DefaultGameMode}}, new Dictionary<byte, object> {{PropertyCode.PlayerProperties.GameMode, (byte)GameMode.DefaultGameMode}});

            server.Start();

        }
        
    }
}