using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Shaman.Common.Http;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Game;
using Shaman.Game.Api;
using Shaman.Game.Configuration;
using Shaman.Game.Providers;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.LiteNetLibAdapter;
using Shaman.Routing.Common.Actualization;
using Shaman.Serialization;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Launchers.Common
{
    public class GameStartup : StartupBase

    {
    public GameStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        ConfigureCommonServices(services);

        services.AddSingleton(c =>
            ((GameApplicationConfig) c.GetRequiredService<IApplicationConfig>()).GetBundleConfig());
        services.AddScoped<IRoomPropertiesContainer, RoomPropertiesContainer>();
        services.AddSingleton<IRoomManager, RoomManager>();
        services.AddSingleton<IApplication, GameApplication>();
        services.AddSingleton<IStatisticsProvider, StatisticsProvider>();
        services.AddSingleton<IShamanComponents, ShamanComponents>();
        services.AddSingleton<IGameServerApi, GameServerApi>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
        IServerActualizer serverActualizer, IRoomControllerFactory controllerFactory /* init bundle */,
        IGameServerApi gameServerApi, IShamanLogger logger)
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

        serverActualizer.Start(Convert.ToInt32(Configuration["ActualizationTimeoutMs"]));
    }

    [Conditional("DEBUG")]
    public void CheckProductionCompiledInRelease(IShamanLogger logger)
    {
        logger.Error("ATTENTION!!! Release Environment compiled in DEBUG mode!");
    }
    }
}