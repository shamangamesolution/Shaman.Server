using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shaman.Client.Peers;
using Shaman.Launchers.Game.Standalone;
using Shaman.ServiceBootstrap;

namespace Shaman.Launchers.Tests
{
    [TestFixture]
    public class DebugServerTests
    {
        private bool _isLaunched = false;
        private ShamanClientFactory _clientFactory = new ShamanClientFactory();
        
        private IConfigurationRoot GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.standalone.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
        }
        
        [SetUp]
        public async Task Setup()
        {
            if (!_isLaunched)
            {
                Task.Factory.StartNew(() =>
                    Bootstrap.Launch<Startup>(GetConfig()));
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
            var joinInfoList = new HashSet<Guid>();
            var joinProperties = new Dictionary<byte, object>();
            for (int i = 0; i < 10; i++)
            {
                clients.Add(_clientFactory.GetClient(), Guid.NewGuid());
            }
            await Task.Delay(3000);
            foreach (var client in clients)
            {
                var joinInfo = await client.Key.DirectConnectToGameServerToRandomRoom("127.0.0.1", 23452, client.Value, mmProperties,
                    joinProperties);
                joinInfoList.Add(joinInfo.RoomId);
            }

            await Task.Delay(5000);
            
            foreach(var client in clients)
                Assert.AreEqual(ShamanClientStatus.InRoom,  client.Key.GetStatus());
            
            Assert.AreEqual(10, joinInfoList.Count);
            Assert.AreNotEqual(Guid.Empty, joinInfoList.First());
        }
    }
}