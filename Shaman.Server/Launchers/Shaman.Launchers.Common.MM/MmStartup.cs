using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.MM;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.MM.Providers;
using Shaman.MM.Rooms;
using Shaman.Routing.Common.Actualization;
using Shaman.Routing.Common.MM;

namespace Shaman.Launchers.Common.MM
{
    public class MmStartup : StartupBase
    {
        public MmStartup(IConfiguration configuration)
            :base(configuration)
        {
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            ConfigureCommonServices(services, "Shaman.MM");
            
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
            IBundleInfoProvider bundleInfoProvider, IServerActualizer serverActualizer, IMatchMakerServerInfoProvider serverInfoProvider, IBundleLoader bundleLoader)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            
            bundleLoader.LoadBundle();
            
            
            var bundleUri = bundleInfoProvider.GetBundleUri().Result;
            var resolver = bundleLoader.LoadTypeFromBundle<IMmResolver>();
            // var resolver = BundleHelper.LoadTypeFromBundle<IMmResolver>(bundleUri, Convert.ToBoolean(Configuration["OverwriteDownloadedBundle"]));
            RoomPropertiesProvider.RoomPropertiesProviderImplementation = resolver.GetRoomPropertiesProvider();
            resolver.Configure(matchMaker);
            
            serverInfoProvider.Start();
            serverActualizer.Start(Convert.ToInt32(Configuration["ServerSettings:ActualizationIntervalMs"]));

            server.Start();
        }
        
    }
}