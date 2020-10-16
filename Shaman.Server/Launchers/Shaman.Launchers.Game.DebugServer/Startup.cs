using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.Meta;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.Launchers.Common.Game;

namespace Shaman.Launchers.Game.DebugServer
{
    public class Startup : GameStartup
    {

        public Startup(IConfiguration configuration) : base(configuration)
        {
        }
        
        /// <summary>
        /// DI for services used in StandAlone launcher
        /// </summary>
        /// <param name="services"></param>
        public override void ConfigureServices(IServiceCollection services)
        {
            //configure all services related to common Game server launchers 
            base.ConfigureServices(services);
            
            //settings
            ConfigureSettings<ApplicationConfig>(services);

            //in standalone game server mode we use special room controller factory, which gets bundle from Standalone launcher
            //other types of launchers get bundle via file directory or http request
            services.AddSingleton<IRoomControllerFactory, StandaloneModeRoomControllerFactory>();
            //game server core depend on Metrics, so we pass a stub here, because we do not need to send metrics in case of standalone launcher by default
            //if standalone launcher will be used in production environment - this dep should be reinjected on bundle level
            services.AddSingleton<IGameMetrics, GameMetricsStub>();
            //the same for this - we need it, so we pass stub here
            services.AddSingleton<IRoomStateUpdater, RoomStateUpdaterStub>();
            //bundle setup - this is used for getting bundle settings. This implementation gets settings from exe directory
            //because in standalone mode bundle is early bound and its settings are copied to directory we start from
            services.AddSingleton<IBundleSettingsProvider, BundleSettingsFromFileProvider>();
            //bundle configuration - get setting value from config
            services.AddSingleton<IBundleConfig, BundleConfig>();
            //meta
            services.AddSingleton<IMetaProvider, StandAloneMetaProvider>();
        }
        
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server, IShamanLogger logger)
        {
            base.ConfigureGame(app, env, server, logger);
        }
    }
}