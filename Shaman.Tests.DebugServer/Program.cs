using Shaman.Common.Server.Configuration;
using Shaman.Launchers.Game.DebugServer;
using SocketType = System.Net.Sockets.SocketType;

namespace Shaman.Tests.DebugServer;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ApplicationConfig()
        {
            ServerName = "TestGame",
            Region = "SomeRegion",
            PublicDomainNameOrAddress = "localhost",
            ListenPorts = "64001/udp,64002/ws",
            BindToPortHttp = 7005,
            SocketTickTimeMs = 100,
            ReceiveTickTimeMs = 33,
            SendTickTimeMs = 50,
            MaxPacketSize = 300,
            BasePacketBufferSize = 64,
            IsAuthOn = false,
            IsConnectionDdosProtectionOn = true,
            MaxConnectsFromSingleIp = 10,
            ConnectionCountCheckIntervalMs = 10000,
            BanDurationMs = 3600,
            BanCheckIntervalMs = 10000,
        };

        var result = DebugServerLauncher.Launch(new Launchers.Tests.TestBundle.Game(), config, "0.0.0.0", "Error");

        await result.ServerTask;
    }
}