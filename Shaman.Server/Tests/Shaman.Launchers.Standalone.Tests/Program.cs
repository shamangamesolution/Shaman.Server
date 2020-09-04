using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Common.Server.Configuration;
using Shaman.Launchers.Game.Standalone;

namespace Shaman.Launchers.Standalone.Tests
{

    
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ApplicationConfig()
            {
                ServerName = "TestGame",
                Region = "SomeRegion",
                PublicDomainNameOrAddress = "localhost",
                ListenPorts = new List<ushort> {23453},
                BindToPortHttp = 7005,
                SocketTickTimeMs = 100,
                ReceiveTickTimeMs = 33,
                SendTickTimeMs = 50,
                MaxPacketSize = 300,
                BasePacketBufferSize = 64,
                IsAuthOn = false,
                SocketType = SocketType.BareSocket
            };
            
            var result = StandaloneServerLauncher.Launch(new TestBundle.Game(), args, config, "0.0.0.0", "Error");
            result.ServerTask.Wait();

        }
    }
}