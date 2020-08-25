using System;
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
using Shaman.LiteNetLibAdapter;
using Shaman.MM;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.Routing.Common.Actualization;
using Shaman.Routing.Common.MM;
using Shaman.Serialization;

namespace Shaman.Launchers.Common
{
    public class MmStartup : StartupBase
    {
        public MmStartup(IConfiguration configuration)
            :base(configuration)
        {
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            
            services.AddSingleton<IMatchMaker, MatchMaker>();    
            services.AddSingleton<IApplication, MmApplication>();
            services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
            services.AddSingleton<IMatchMakingGroupsManager, MatchMakingGroupManager>();
            services.AddSingleton<IPlayersManager, PlayersManager>();
            services.AddSingleton<IRoomManager, RoomManager>();
            services.AddSingleton<IRoomPropertiesProvider, RoomPropertiesProvider>();
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
            
            var bundleUri = bundleInfoProvider.GetBundleUri().Result;
            var resolver = BundleHelper.LoadTypeFromBundle<IMmResolver>(bundleUri, Convert.ToBoolean(Configuration["OverwriteDownloadedBundle"]));
            RoomPropertiesProvider.RoomPropertiesProviderImplementation = resolver.GetRoomPropertiesProvider();
            resolver.Configure(matchMaker);
            
            serverInfoProvider.Start();
            serverActualizer.Start(Convert.ToInt32(Configuration["ActualizationTimeoutMs"]));

            server.Start();
        }
        
    }
}