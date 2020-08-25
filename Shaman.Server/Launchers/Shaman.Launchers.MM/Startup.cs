using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Shaman.Bundling.Common;
using Shaman.Common.Http;
using Shaman.Common.Metrics;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.Launchers.Common;
using Shaman.LiteNetLibAdapter;
using Shaman.MM;
using Shaman.MM.Configuration;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.Routing.Common.Actualization;
using Shaman.Routing.Common.MM;
using Shaman.Serialization;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Launchers.MM
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
            
            //install deps specific to launcher
            services.Configure<MmApplicationConfig>(Configuration);
            var ports = Configuration["Ports"].Split(',').Select(s => Convert.ToUInt16(s)).ToList();

            services.AddSingleton<IShamanLogger, SerilogLogger>();//(l => new SerilogLogger(logLevel:LogLevel.Error | LogLevel.Info));            
            services.AddSingleton<IApplicationConfig>(c => 
                new MmApplicationConfig(
                    Configuration["Region"],
                    Configuration["PublicDomainNameOrAddress"],
                    ports, 
                    Configuration["RouterUrl"],
                    Convert.ToInt32(Configuration["ServerUnregisterTimeoutMs"]),
                    Configuration["name"], 
                    Convert.ToUInt16(Configuration["BindToPortHttp"]),
                    Convert.ToInt32(Configuration["SocketTickTimeMs"]),
                    Convert.ToInt32(Configuration["ReceiveTickTimeMs"]),
                    Convert.ToInt32(Configuration["SendTickTimeMs"]),
                    Convert.ToBoolean(Configuration["AuthOn"]), 
                    Configuration["Secret"],
                    serverInfoListUpdateIntervalMs: Convert.ToInt32(Configuration["ServerInfoListUpdateIntervalMs"]),
                    actualizationIntervalMs: Convert.ToInt32(Configuration["ActualizationIntervalMs"])
                ));    
            
            services.AddSingleton<IMatchMakerServerInfoProvider, DefaultMatchMakerServerInfoProvider>();
            services.AddSingleton<IRoomApiProvider, DefaultRoomApiProvider>();
            services.AddSingleton<IServerActualizer, DefaultServerActualizer>();
            services.AddSingleton<IBundleInfoProvider, DefaultBundleInfoProvider>();
            
            ConfigureMetrics(services);
        }
        

        
    }
}