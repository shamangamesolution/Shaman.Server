using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shaman.Client.Peers;
using Shaman.Common.Server.Configuration;
using Shaman.Launchers.Game.DebugServer;
using Shaman.Launchers.Tests.Common;

namespace Shaman.Launchers.Tests
{
    [TestFixture]
    public class StandAloneGameServerTests
    {
        private bool _isLaunched = false;
        private readonly ShamanClientFactory _clientFactory = new ShamanClientFactory();

        private void LaunchDebugServer()
        {
            var config = new ApplicationConfig()
            {
                ServerName = "TestGame",
                Region = "SomeRegion",
                PublicDomainNameOrAddress = "localhost",
                ListenPorts = "23452",
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
            var rooms = new HashSet<Guid>();
            var mmProperties = new Dictionary<byte, object>();
            var joinProperties = new Dictionary<byte, object>();
            for (int i = 0; i < 10; i++)
            {
                clients.Add(_clientFactory.GetClient(), Guid.NewGuid());
            }



            foreach (var client in clients)
            {
                var joinInfo = await client.Key.DirectConnectToGameServerToRandomRoom("127.0.0.1", 23452,
                    client.Value, mmProperties, joinProperties);
                rooms.Add(joinInfo.RoomId);
            }

            await Task.Delay(3000);

            foreach (var client in clients)
            {
                Assert.AreEqual(ShamanClientStatus.InRoom, client.Key.GetStatus());
            }
            
            Assert.AreEqual(2, rooms.Count);
            Assert.AreNotEqual(Guid.Empty, rooms.First());
        }
    }
}