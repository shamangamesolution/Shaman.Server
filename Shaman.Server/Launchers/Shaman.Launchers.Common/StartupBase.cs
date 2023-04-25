using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Serilog;
using Shaman.Common.Http;
using Shaman.Common.Metrics;
using Shaman.Common.Mvc;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Protection;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Launchers.Common
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        private readonly SnakeCaseNamingStrategy _newtonsoftSnakeCaseNamingStrategy = new();
        public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            // todo remove newtonsoft dep
            return _newtonsoftSnakeCaseNamingStrategy.GetPropertyName(name, false);
        }
    }
    /// <summary>
    /// base configuration for all types of launchers
    /// </summary>
    public class StartupBase
    {
        protected IConfiguration Configuration { get; }

        public StartupBase(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        
        /// <summary>
        /// DI for services - used in all types of launchers
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblyName"></param>
        public void ConfigureCommonServices(IServiceCollection services, string assemblyName)
        {
            services.AddOptions();
            var assembly = Assembly.Load(assemblyName);

            services.AddControllers(options => options.AddShamanMvc())
                .AddApplicationPart(assembly)
                .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy());
            
            //logger
            services.AddSingleton<IShamanLogger, SerilogLogger>();
            //part of config responsible for packets sending
            services.AddSingleton<IPacketSenderConfig>(c => c.GetRequiredService<IApplicationConfig>());
            //part of config responsible for ddos protection
            services.AddSingleton<IProtectionManagerConfig>(c => c.GetRequiredService<IApplicationConfig>());
            //packet sender itself
            services.AddSingleton<IPacketSender, PacketBatchSender>();
            //serializer - binary by default
            services.AddSingleton<ISerializer, BinarySerializer>();
            //factory which produces sockets - should be changed in case of using other adapters
            services.AddSingleton<IServerTransportLayerFactory, MultiProtocolTransportLayerFactory>();            
            //factory for task schedulers
            services.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();   
            //sends request between subsystems
            services.AddSingleton<IRequestSender, HttpSender>();
            //low level byte sender used in shaman core for serializing and sending packets
            services.AddSingleton<IShamanSender, ShamanSender>();
            //higher level message sender
            services.AddSingleton<IShamanMessageSender, ShamanMessageSender>();
            //factory for producing senders - you may reinject it in bundle for use of another types of senders and serializers
            services.AddSingleton<IShamanMessageSenderFactory, ShamanMessageSenderFactory>();
            //part of protection system responsible for connection ddos protection
            services.AddSingleton<IConnectDdosProtection, ConnectDdosProtection>();
            //protection manager itself
            services.AddSingleton<IProtectionManager, ProtectionManager>();
        }

        /// <summary>
        /// deserializing of appsettings configs into IApplicationConfig implementation - used in all types of launchers
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="T"></typeparam>
        protected void ConfigureSettings<T>(IServiceCollection services) where T:ApplicationConfig, new()
        {
            services.Configure<T>(Configuration);
            var settings = new T();
            var configurationSection = Configuration.GetSection("CommonSettings");
            configurationSection.Bind(settings);
            settings.ListenPorts = Configuration["CommonSettings:ListenPorts"];
            services.AddSingleton<IApplicationConfig>(c => settings);
        }

        
        /// <summary>
        /// injecting Metrics related dependencies
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
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

        /// <summary>
        /// Common middleware configuration - used for all types of launchers
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="server"></param>
        /// <param name="logger"></param>
        protected void ConfigureCommon(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IShamanLogger logger)
        {
            if (!env.IsDevelopment())
                CheckProductionCompiledInRelease(logger);

            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseEndpoints(builder =>
                builder.MapControllerRoute("default", "{action=Index}"));
            server.Start();
        }
        
        [Conditional("DEBUG")]
        public void CheckProductionCompiledInRelease(IShamanLogger logger)
        {
            logger.Error("ATTENTION!!! Release Environment compiled in DEBUG mode!");
        }
    }
}