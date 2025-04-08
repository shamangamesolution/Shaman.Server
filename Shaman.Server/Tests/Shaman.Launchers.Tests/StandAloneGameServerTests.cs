using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shaman.Client.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Launchers.Game.Standalone;
using Shaman.Launchers.TestBundle;
using Shaman.Launchers.Tests.Common;
using Shaman.ServiceBootstrap;
using Shaman.TestTools.ClientPeers;

namespace Shaman.Launchers.Tests
{
    [TestFixture]
    public class DebugServerTests
    {
        private bool _isLaunched = false;
        private readonly ShamanClientFactory _clientFactory = new ShamanClientFactory();
        
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
        public async Task DisconnectTests()
        {
            var peerListener = new TestDisconnectClientPeerListener(new ConsoleLogger());
            var client = _clientFactory.GetClient(peerListener);
            var mmProperties = new Dictionary<byte, object>();
            var joinProperties = new Dictionary<byte, object>();
            var joinInfo = await client.DirectConnectToGameServerToRandomRoom("127.0.0.1", 23452, Guid.NewGuid(), mmProperties,
                joinProperties);
            await Task.Delay(1000);
            Assert.AreEqual(ShamanClientStatus.InRoom,  client.GetStatus());
            client.Disconnect();
            await Task.Delay(3000);
            Assert.AreEqual(ShamanClientStatus.Offline,  client.GetStatus());
            Assert.IsTrue(peerListener.WasDisconnectFired);
        }

        private void OnTestEventReceived(TestEvent eve)
        {
            Assert.AreEqual(444, eve.IntValue);
        }
        
        
        
        [Test]
        public async Task JoinRoomTests()
        {
            var clients = new Dictionary<IShamanClientPeer, Guid>();
            var joinInfoList = new HashSet<Guid>();
            var roomPlayers = new Dictionary<IShamanClientPeer, Guid>();
            var testEventsReceivedTimes = new ConcurrentDictionary<IShamanClientPeer, int>();
            
            for (int i = 0; i < 10; i++)
            {
                clients.Add(_clientFactory.GetClient(), Guid.NewGuid());
            }
            await Task.Delay(3000);
            foreach (var client in clients)
            {
                var joinInfo = await client.Key.DirectConnectToGameServerToRandomRoom("127.0.0.1", 23452, client.Value, new Dictionary<byte, object>(),
                    new Dictionary<byte, object>());
                joinInfoList.Add(joinInfo.RoomId);
                roomPlayers[client.Key] = joinInfo.RoomId;
                client.Key.RegisterOperationHandler<TestEvent>(eve =>
                {
                    OnTestEventReceived(eve);
                    if (!testEventsReceivedTimes.TryAdd(client.Key, 1))
                        testEventsReceivedTimes[client.Key] += 1;
                });
            }

            await Task.Delay(5000);
            
            foreach(var client in clients)
                Assert.AreEqual(ShamanClientStatus.InRoom,  client.Key.GetStatus());

            foreach (var client in clients)
            {
                client.Key.SendEvent(new TestEvent() {IntValue = 444});
            }
            
            await Task.Delay(5000);
            
            Assert.AreEqual(10, testEventsReceivedTimes.Count);
            Assert.IsTrue(testEventsReceivedTimes.All(i => i.Value == roomPlayers.Count(r => r.Value == roomPlayers[i.Key])));
            Assert.AreEqual(2, joinInfoList.Count, "Because 2 rooms should have been created (see OnStart method of bundle)");
            Assert.AreNotEqual(Guid.Empty, joinInfoList.First());
        }
    }
}