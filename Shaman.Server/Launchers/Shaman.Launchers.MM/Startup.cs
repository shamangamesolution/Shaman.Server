using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Bundling.Common;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.Launchers.Common.MM;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.MM.Rooms;
using Shaman.Routing.Common.Actualization;
using Shaman.Routing.Common.MM;
using Shaman.ServiceBootstrap.Logging;

namespace Shaman.Launchers.MM
{
    public class Startup : MmStartup
    {
        public Startup(IConfiguration configuration)
            :base(configuration)
        {
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public override void ConfigureServices(IServiceCollection services)
        {
            //install common deps
            base.ConfigureServices(services);
            
            //settings
            ConfigureSettings<ApplicationConfig>(services);

            //install deps specific to launcher
            services.AddSingleton<IMatchMakerServerInfoProvider, DefaultMatchMakerServerInfoProvider>();
            services.AddSingleton<IRoomApiProvider, DefaultRoomApiProvider>();
            services.AddSingleton<IServerActualizer, DefaultServerActualizer>();
            services.AddSingleton<IDefaultBundleInfoConfig, DefaultBundleInfoConfig>(c =>
                new DefaultBundleInfoConfig(Configuration["LauncherSettings:BundleUri"],
                    Convert.ToBoolean(Configuration["LauncherSettings:OverwriteDownloadedBundle"])));
            services.AddSingleton<IBundleInfoProvider, DefaultBundleInfoProvider>();
            services.AddSingleton<IBundleLoader, BundleLoader>();

            //metrics
            ConfigureMetrics<IMmMetrics, MmMetrics>(services);
        }
        

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplication server,
            IShamanLogger logger, IMatchMaker matchMaker, IMatchMakerServerInfoProvider serverInfoProvider, IBundleLoader bundleLoader)
        {
            base.ConfigureMm(app, env, server, logger, matchMaker, serverInfoProvider, bundleLoader);
        }
    }
}