using System;
using Microsoft.Extensions.DependencyInjection;

namespace Shaman.Contract.Bundle.DI
{
    public abstract class GameBundleBase<TRoomFactoryImpl> : IGameBundle
        where TRoomFactoryImpl : class, IRoomControllerFactory
    {
        private ServiceProvider _serviceProvider;
        private IShamanComponents _shamanComponents;

        public IRoomControllerFactory GetRoomControllerFactory()
        {
            return _serviceProvider.GetService<IRoomControllerFactory>();
        }

        public void Initialize(IShamanComponents shamanComponents)
        {
            var serviceCollection = new ServiceCollection();
            _shamanComponents = shamanComponents;

            serviceCollection.AddTransient<IRoomControllerFactory, TRoomFactoryImpl>();
            serviceCollection.AddTransient((c) => shamanComponents.Logger);
            serviceCollection.AddTransient((c) => shamanComponents.Config);
            serviceCollection.AddTransient((c) => shamanComponents.MetaProvider);
            serviceCollection.AddTransient((c) => shamanComponents.GameServerApi);

            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public void OnStart()
        {
            OnStart(_serviceProvider, _shamanComponents);
        }

        protected abstract void ConfigureServices(IServiceCollection serviceCollection);
        protected abstract void OnStart(IServiceProvider serviceProvider, IShamanComponents shamanComponents);
    }
}