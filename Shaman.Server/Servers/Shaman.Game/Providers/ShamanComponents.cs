using System;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;

namespace Shaman.Game.Providers
{
    public class ShamanComponents : IShamanComponents
    {
        private readonly IServiceProvider _serviceProvider;

        public ShamanComponents(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IShamanLogger Logger => _serviceProvider.GetService<IShamanLogger>();
        public IConfig Config => _serviceProvider.GetService<IConfig>();
    }
}