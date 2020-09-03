using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.LiteNetLibAdapter;
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
        
        public void ConfigureCommonServices(IServiceCollection services, string assemblyName)
        {
            services.AddOptions();
            var assembly = Assembly.Load(assemblyName);
            
            services.AddMvc()
                .AddApplicationPart(assembly)
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
            Configuration.GetSection("CommonSettings").Bind(settings);
            var ports = Configuration["CommonSettings:ListenPorts"].Split(',').Select(s => Convert.ToUInt16(s)).ToList();
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
        
        protected void ConfigureCommon(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IShamanLogger logger)
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
        }
        
        [Conditional("DEBUG")]
        public void CheckProductionCompiledInRelease(IShamanLogger logger)
        {
            logger.Error("ATTENTION!!! Release Environment compiled in DEBUG mode!");
        }
    }
}