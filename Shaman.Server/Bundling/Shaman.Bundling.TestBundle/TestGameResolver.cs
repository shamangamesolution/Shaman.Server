using System;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Contract.Bundle.DI;

namespace Shaman.Bundling.TestBundle
{
    public class TestGameResolver : GameBundleBase
    {
        protected override void OnConfigureServices(IServiceCollection serviceCollection)
        {
        }

        protected override void OnStart(IServiceProvider serviceProvider)
        {
        }
    }
}