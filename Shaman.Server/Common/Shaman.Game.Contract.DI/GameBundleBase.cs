using System;
using Microsoft.Extensions.DependencyInjection;

namespace Shaman.Game.Contract.DI
{
    public abstract class GameBundleBase : IGameBundle
    {
        private ServiceProvider _serviceProvider;

        public IGameModeControllerFactory GetGameModeControllerFactory()
        {
            return _serviceProvider.GetService<IGameModeControllerFactory>();
        }

        public void OnInitialize(IShamanComponents shamanComponents)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient((c) => shamanComponents.RequestSender);
            serviceCollection.AddTransient((c) => shamanComponents.Logger);
            serviceCollection.AddTransient((c) => shamanComponents.Serializer);
            serviceCollection.AddTransient((c) => shamanComponents.ApplicationCoreConfig);

            OnConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            OnStart(_serviceProvider);
        }

        protected abstract void OnConfigureServices(IServiceCollection serviceCollection);
        protected abstract void OnStart(IServiceProvider serviceProvider);
    }
}