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
using Shaman.Contract.Routing;
using Shaman.MM;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.MM.Providers;
using Shaman.MM.Rooms;
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
            ConfigureCommonServices(services, LauncherHelpers.GetAssemblyName(ServerRole.MatchMaker));
            
            services.AddSingleton<IMatchMaker, MatchMaker>();    
            services.AddSingleton<IApplication, MmApplication>();
            services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
            services.AddSingleton<IMatchMakingGroupsManager, MatchMakingGroupManager>();
            services.AddSingleton<IPlayersManager, PlayersManager>();
            services.AddSingleton<IRoomManager, RoomManager>();
            services.AddSingleton<IRoomPropertiesProvider, RoomPropertiesProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void ConfigureMm(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IShamanLogger logger, IMatchMaker matchMaker, IMatchMakerServerInfoProvider serverInfoProvider, IBundleLoader bundleLoader)
        {
            //load bundle
            bundleLoader.LoadBundle();
            
            //resolve main bundle type and configure 
            var resolver = bundleLoader.LoadTypeFromBundle<IMmResolver>();
            RoomPropertiesProvider.RoomPropertiesProviderImplementation = resolver.GetRoomPropertiesProvider();
            resolver.Configure(matchMaker);
            
            //start game server info provider
            serverInfoProvider.Start();
            
            base.ConfigureCommon(app, env, server, logger);
        }
        
    }
}