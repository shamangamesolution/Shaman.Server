using System;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Game.Contract;

namespace Game.Bundle
{
    public class MyGameResolver : IGameResolver
    {
        public void Configure(IServiceCollection services)
        {
            services.AddTransient<IGameModeControllerFactory, MyGameControllerFactory>();
        }

        public void OnInitialize(IServiceProvider serviceProvider)
        {
            // todo here you can start some background tasks  
        }
    }
}