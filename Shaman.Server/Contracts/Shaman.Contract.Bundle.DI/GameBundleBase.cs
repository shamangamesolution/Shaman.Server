using System;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Contract.Bundle;

namespace Shaman.Game.Contract.DI
{
    public abstract class GameBundleBase : IGameBundle
    {
        private ServiceProvider _serviceProvider;

        public IRoomControllerFactory GetRoomControllerFactory()
        {
            return _serviceProvider.GetService<IRoomControllerFactory>();
        }

        public void OnInitialize(IShamanComponents shamanComponents)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient((c) => shamanComponents.Logger);
            serviceCollection.AddTransient((c) => shamanComponents.BackendProvider);

            OnConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            OnStart(_serviceProvider);
        }

        protected abstract void OnConfigureServices(IServiceCollection serviceCollection);
        protected abstract void OnStart(IServiceProvider serviceProvider);
    }
}