using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;
using Shaman.Common.Http;
using Shaman.Common.Metrics;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Common.Server.Providers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.LiteNetLibAdapter;
using Shaman.MM;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.Serialization;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Launchers.Common
{
    public class StartupBase
    {
        protected IConfiguration Configuration { get; }

        public StartupBase(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public void ConfigureCommonServices(IServiceCollection services)
        {
            services.AddOptions();
            var assembly1 = Assembly.Load("Shaman.Game");
            var assembly2 = Assembly.Load("Shaman.MM");
            
            services.AddMvc()
                .AddApplicationPart(assembly1)
                .AddApplicationPart(assembly2)
                .AddControllersAsServices()
                .AddJsonOptions(o =>
            {
                o.SerializerSettings.ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };
            });
            
            services.AddSingleton<IShamanLogger, SerilogLogger>();
            services.AddSingleton<IPacketSenderConfig>(c => c.GetRequiredService<IApplicationConfig>()); 
            services.AddSingleton<IPacketSender, PacketBatchSender>();
            services.AddTransient<IShamanMessageSenderFactory, ShamanMessageSenderFactory>();
            services.AddSingleton<ISerializer, BinarySerializer>();            
            services.AddSingleton<ISocketFactory, LiteNetSockFactory>();            
            services.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();            
            services.AddSingleton<IRequestSender, HttpSender>();            
            services.AddSingleton<IShamanSender, ShamanSender>();
            services.AddSingleton<IShamanMessageSender, ShamanMessageSender>();
            services.AddSingleton<IShamanMessageSenderFactory, ShamanMessageSenderFactory>();
        }

        protected void ConfigureSettings<T>(IServiceCollection services) where T:ApplicationConfig, new()
        {
            services.Configure<T>(Configuration);
            var settings = new T();
            Configuration.GetSection("ServerSettings").Bind(settings);
            var ports = Configuration["ServerSettings:ListenPorts"].Split(',').Select(s => Convert.ToUInt16(s)).ToList();
            settings.ListenPorts = ports;
            services.AddSingleton<IApplicationConfig>(c => settings);
        }

        protected void ConfigureMetrics<TService, TImplementation>(IServiceCollection services)
            where TService : class
            where TImplementation:class, TService
        {
            var metricsSettings = new MetricsSettings();
            Configuration.GetSection("Metrics").Bind(metricsSettings);
            var metricsAgent = new MetricsAgent(metricsSettings);
            services.AddSingleton<IMetricsAgent>(metricsAgent);
            services.AddSingleton<TService, TImplementation>();
        }
    }
}