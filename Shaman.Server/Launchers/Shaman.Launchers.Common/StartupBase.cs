using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Shaman.Common.Http;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
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
        public void ConfigureCommonServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMvc().AddJsonOptions(o =>
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
    }
}