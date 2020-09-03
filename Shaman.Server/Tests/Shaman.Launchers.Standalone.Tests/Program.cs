using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Launchers.Game.Standalone;

namespace Shaman.Launchers.Standalone.Tests
{

    
    class Program
    {
        static void Main(string[] args)
        {
            var result = StandaloneServerLauncher.Launch(new TestBundle.Game(), args, "TestGame", "SomeRegion", "localhost",
                new List<ushort> {23453}, 7005);
            result.ServerTask.Wait();

        }
    }
}