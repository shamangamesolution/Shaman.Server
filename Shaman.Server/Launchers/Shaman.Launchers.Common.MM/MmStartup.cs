using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Providers;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.Contract.Routing;
using Shaman.Contract.Routing.MM;
using Shaman.MM;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.MM.Providers;
using Shaman.MM.Rooms;

namespace Shaman.Launchers.Common.MM
{
    public class MmStartup : StartupBase
    {
        public MmStartup(IConfiguration configuration)
            :base(configuration)
        {
        }

        /// <summary>
        /// DI for services used in MatchMaker types of launchers
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            ConfigureCommonServices(services, LauncherHelpers.GetAssemblyName(ServerRole.MatchMaker));
            
            //matchmaker - choose correct MM group for player
            services.AddSingleton<IMatchMaker, MatchMaker>();
            //MM server itself
            services.AddSingleton<IApplication, MmApplication>();
            //stats provider - used for determine peer count on server
            services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
            //manager responsible for a number of matchmaking groups, running on MM
            services.AddSingleton<IMatchMakingGroupsManager, MatchMakingGroupManager>();
            //manages players collection on MM
            services.AddSingleton<IPlayersManager, PlayersManager>();
            //manages room collection for all game servers connected to MM
            services.AddSingleton<IRoomManager, RoomManager>();
            //provides some properties for creating rooms
            services.AddSingleton<IRoomPropertiesProvider, RoomPropertiesProvider>();
        }

        /// <summary>
        /// MM related middleware configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="server"></param>
        /// <param name="logger"></param>
        /// <param name="matchMaker"></param>
        /// <param name="serverInfoProvider"></param>
        /// <param name="bundleLoader"></param>
        public void ConfigureMm(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IShamanLogger logger, IMatchMaker matchMaker, IMatchMakerServerInfoProvider serverInfoProvider, IBundleLoader bundleLoader)
        {
            //resolve main bundle type and configure it
            //in case of matchmaker we can load bundle during this stage
            var resolver = bundleLoader.LoadTypeFromBundle<IMmResolver>();
            RoomPropertiesProvider.RoomPropertiesProviderImplementation = resolver.GetRoomPropertiesProvider();
            resolver.Configure(matchMaker);
            
            //start game server info provider - gathers info about game servers connected to this matchmaker
            serverInfoProvider.Start();
            
            base.ConfigureCommon(app, env, server, logger);
        }
        
    }
}