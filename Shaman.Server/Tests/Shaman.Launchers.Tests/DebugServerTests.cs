using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using NUnit.Framework;
using Shaman.Client.Peers;
using Shaman.Common.Server.Configuration;
using Shaman.Contract.Routing;
using Shaman.Launchers.Game.DebugServer;
using Shaman.ServiceBootstrap;
using Startup = Shaman.Launchers.Game.Standalone.Startup;

namespace Shaman.Launchers.Tests
{
    [TestFixture]
    public class StandAloneGameServerTests
    {
        private bool _isLaunched = false;
        private ShamanClientFactory _clientFactory = new ShamanClientFactory();

        private void LaunchDebugServer()
        {
            var config = new ApplicationConfig()
            {
                ServerName = "TestGame",
                Region = "SomeRegion",
                PublicDomainNameOrAddress = "localhost",
                ListenPorts = new List<ushort> {23452},
                BindToPortHttp = 7005,
                SocketTickTimeMs = 100,
                ReceiveTickTimeMs = 33,
                SendTickTimeMs = 50,
                MaxPacketSize = 300,
                BasePacketBufferSize = 64,
                IsAuthOn = false,
                SocketType = SocketType.BareSocket
            };
            
            var result = StandaloneServerLauncher.Launch(new TestBundle.Game(), null, config, "0.0.0.0", "Error");
            result.ServerTask.Wait();
        }
        
        [SetUp]
        public async Task Setup()
        {
            if (!_isLaunched)
            {
                Task.Factory.StartNew(LaunchDebugServer);
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
            for (int i = 0; i < 10; i++)
            {
                clients.Add(_clientFactory.GetClient(), Guid.NewGuid());
            }

            foreach (var client in clients)
                client.Key.DirectConnectToGameServerToRandomRoom("127.0.0.1", 23452, client.Value, mmProperties, joinProperties);
            
            await Task.Delay(3000);
            
            foreach(var client in clients)
                Assert.AreEqual(ShamanClientStatus.InRoom,  client.Key.GetStatus());
        }
    }
}