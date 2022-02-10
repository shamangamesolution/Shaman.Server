using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using NUnit.Framework;
using Shaman.Client.Peers;
using Shaman.Contract.Routing;
using Shaman.Launchers.Game.Standalone;
using Shaman.Launchers.Tests.Common;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Tests
{
    public static class LocalPairLauncher
    {
        public static void Launch()
        {
            var gameTask = LaunchGame();
            var mmTask = LaunchMm();

            gameTask.Wait();
            mmTask.Wait();
        }

        private static IConfigurationRoot GetConfig(string role)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.pair.{role}.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
        }
        
        private static Task LaunchMm()
        {
            var mmTask = Task.Factory.StartNew(() => Bootstrap.Launch<Launchers.MM.Startup>(GetConfig(ServerRole.MatchMaker.ToString())));
            return mmTask;
        }

        private static Task LaunchGame()
        {
            var gameTask =
                Task.Factory.StartNew(() => Bootstrap.Launch<Launchers.Game.Startup>(GetConfig(ServerRole.GameServer.ToString())));
            return gameTask;
        }
    }
    
    [TestFixture]
    public class PairTests
    {
        private bool _isLaunched = false;
        private ShamanClientFactory _clientFactory = new ShamanClientFactory();
        
        [SetUp]
        public async Task Setup()
        {
            if (!_isLaunched)
            {
                Task.Factory.StartNew(LocalPairLauncher.Launch);
                await Task.Delay(3000);
                _isLaunched = true;
            }
                
            await Task.Delay(3000);
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public async Task JoinRoomTests()
        {
            var clients = new Dictionary<IShamanClientPeer, Guid>();
            var mmProperties = new Dictionary<byte, object>();
            var joinProperties = new Dictionary<byte, object>();
            var rooms = new HashSet<Guid>();
            for (int i = 0; i < 10; i++)
            {
                clients.Add(_clientFactory.GetClient(), Guid.NewGuid());
            }

            foreach (var client in clients)
            {
                var joinGame = await client.Key.JoinGame("127.0.0.1", 23453, client.Value, mmProperties, joinProperties);
                rooms.Add(joinGame.RoomId);
            }
            
            await Task.Delay(10000);

            var clientsInRoom = clients.Count(c => c.Key.GetStatus() == ShamanClientStatus.InRoom);
            Assert.AreEqual(clients.Count, clientsInRoom);
            Assert.AreEqual(2, rooms.Count);
            foreach(var client in clients)
            Assert.AreEqual(ShamanClientStatus.InRoom,  client.Key.GetStatus());
        }
    }
}