using System;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;

namespace Shaman.Game.Providers
{
    public class ShamanComponents : IShamanComponents
    {
        private readonly IServiceProvider _serviceProvider;

        public ShamanComponents(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRequestSender RequestSender => _serviceProvider.GetService<IRequestSender>();
        public IShamanLogger Logger => _serviceProvider.GetService<IShamanLogger>();
        public ISerializer Serializer => _serviceProvider.GetService<ISerializer>();
        public IApplicationCoreConfig ApplicationCoreConfig => _serviceProvider.GetService<IApplicationCoreConfig>();
    }
}