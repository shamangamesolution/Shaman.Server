using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Contract.Routing.Actualization;
using Shaman.Game;
using Shaman.Game.Api;
using Shaman.Game.Providers;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.RoomProperties;

namespace Shaman.Launchers.Common.Game
{
    /// <summary>
    /// GameServer role related dependencies
    /// </summary>
    public class GameStartup : StartupBase
    {
        public GameStartup(IConfiguration configuration) : base(configuration)
        {
        }
        
        /// <summary>
        /// DI for services used in GameServer types of launchers
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            ConfigureCommonServices(services, LauncherHelpers.GetAssemblyName(ServerRole.GameServer));

            //container for room properties - it is passed to room controller
            services.AddScoped<IRoomPropertiesContainer, RoomPropertiesContainer>();
            //room manager - responsible for creating, updating and cleanuping rooms on game server 
            services.AddSingleton<IRoomManager, RoomManager>();
            //game server itself
            services.AddSingleton<IApplication, GameApplication>();
            //stats provider - used for determine peer count on server
            services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
            //API for low level components such as logger and config
            services.AddSingleton<IShamanComponents, ShamanComponents>();
            //api for managing high level operations on game server
            services.AddSingleton<IGameServerApi, GameServerApi>();
        }


        /// <summary>
        /// GameServer related middleware configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="server"></param>
        /// <param name="logger"></param>
        public void ConfigureGame(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IShamanLogger logger)
        {
            //call common configuration
            base.ConfigureCommon(app, env, server, logger);
        }
    }
}