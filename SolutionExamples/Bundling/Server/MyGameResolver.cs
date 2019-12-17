using System;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Game.Contract;
using Shaman.Game.Contract.DI;

namespace Server
{
    public class MyGameResolver : GameBundleBase
    {
        protected override void OnConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IGameModeControllerFactory, MyGameControllerFactory>();
        }

        protected override void OnStart(IServiceProvider serviceProvider)
        {
            // todo here you can start some background tasks  
        }
    }
}