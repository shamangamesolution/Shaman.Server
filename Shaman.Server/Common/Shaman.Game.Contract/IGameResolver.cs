using System;
using Microsoft.Extensions.DependencyInjection;

namespace Shaman.Game.Contract
{
    public interface IGameResolver
    {
        void Configure(IServiceCollection services);
        void OnInitialize(IServiceProvider serviceProvider);
    }
}