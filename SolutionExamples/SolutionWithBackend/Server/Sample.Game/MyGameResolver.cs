using System;
using Microsoft.Extensions.DependencyInjection;
using Sample.Game.GamePlay.Controllers;
using Sample.Game.GamePlay.Providers;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Game.Contract.DI;

namespace Sample.Game
{
    public class MyGameResolver : GameBundleBase
    {
        protected override void OnConfigureServices(IServiceCollection serviceCollection)
        {
            try
            {
                //singletons
                serviceCollection.AddSingleton<IStorageContainerUpdater, GameServerStorageUpdater>();
                serviceCollection.AddSingleton<IStorageContainer, GameServerStorageContainer>();
                serviceCollection.AddSingleton<ITaskSchedulerFactory, TaskSchedulerFactory>();

                //transients
                serviceCollection.AddTransient<IGameModeControllerFactory, ApGameModeControllerFactory>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected override void OnStart(IServiceProvider serviceProvider)
        {
            serviceProvider.GetService<IStorageContainer>().Start(string.Empty);
        }
    }
}