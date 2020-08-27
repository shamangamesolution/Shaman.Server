using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Messages;
using Shaman.Common.Server.Providers;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Game;
using Shaman.Game.Api;
using Shaman.Game.Providers;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.Routing.Common.Actualization;

namespace Shaman.Launchers.Common.Game
{
    public class GameStartup : StartupBase
    {
        public GameStartup(IConfiguration configuration) : base(configuration)
        {
        }
        
        public virtual void ConfigureServices(IServiceCollection services)
        {
            ConfigureCommonServices(services, LauncherHelpers.GetAssemblyName(ServerRole.GameServer));

            services.AddScoped<IRoomPropertiesContainer, RoomPropertiesContainer>();
            services.AddSingleton<IRoomManager, RoomManager>();
            services.AddSingleton<IApplication, GameApplication>();
            services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
            services.AddSingleton<IShamanComponents, ShamanComponents>();
            services.AddSingleton<IGameServerApi, GameServerApi>();
        }


        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void ConfigureGame(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IServerActualizer serverActualizer, IShamanLogger logger)
        {
            serverActualizer.Start(Convert.ToInt32(Configuration["ServerSettings:ActualizationIntervalMs"]));
            
            base.ConfigureCommon(app, env, server, logger);
        }
    }
}