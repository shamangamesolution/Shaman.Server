using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bro.WsShamanNetwork;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shaman.Client;
using Shaman.Client.Peers;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Launchers.Game.DebugServer;
using Shaman.Launchers.Tests.Common;
using Shaman.ServiceBootstrap;
using TaskScheduler = Shaman.Common.Utils.TaskScheduling.TaskScheduler;

namespace Shaman.Launchers.Tests
{
    [TestFixture]
    public class DebugGameServerTests
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
                ListenPorts = "23452/udp,23452/ws",
                BindToPortHttp = 7005,
                SocketTickTimeMs = 100,
                ReceiveTickTimeMs = 33,
                SendTickTimeMs = 50,
                MaxPacketSize = 300,
                BasePacketBufferSize = 64,
                IsAuthOn = false,
            };
            
            var result = DebugServerLauncher.Launch(new TestBundle.Game(), config, "0.0.0.0", "Error");
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
                
            await Task.Delay(10000);
        }

        [TearDown]
        public void TearDown()
        {
            
        }
        
        private void OnTestEventReceived(TestEvent eve)
        {
            // Assert.AreEqual(444, eve.IntValue);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task JoinRoomTests(bool udpSocket)
        {
            var clients = new Dictionary<IShamanClientPeer, Guid>();
            var rooms = new HashSet<Guid>();
            var roomPlayers = new Dictionary<IShamanClientPeer, Guid>();
            var testEventsReceivedTimes = new ConcurrentDictionary<IShamanClientPeer, int>();
            
            for (int i = 0; i < 10; i++)
            {
                if (udpSocket)
                    clients.Add(_clientFactory.GetClient(), Guid.NewGuid());
                else
                    clients.Add(_clientFactory.GetClient(new WebSocketClientTransportLayerFactory(new TaskSchedulerFactory(new ConsoleLogger()),TimeSpan.FromSeconds(5))), Guid.NewGuid());
            }

            if (udpSocket)
            {
                foreach (var client in clients)
                {
                    var ping = await client.Key.Ping("127.0.0.1", 23452);
                    Assert.AreNotEqual(0, ping);
                }
            }

            foreach (var client in clients)
            {
                var joinInfo = await client.Key.DirectConnectToGameServerToRandomRoom("127.0.0.1", 23452,
                    client.Value, new Dictionary<byte, object>(), new Dictionary<byte, object>());
                rooms.Add(joinInfo.RoomId);
                roomPlayers[client.Key] = joinInfo.RoomId;
                client.Key.RegisterOperationHandler<TestEvent>(eve =>
                {
                    OnTestEventReceived(eve);
                    if (!testEventsReceivedTimes.TryAdd(client.Key, 1))
                        testEventsReceivedTimes[client.Key] += 1;
                });
            }

            await Task.Delay(3000);

            foreach (var client in clients)
            {
                Assert.AreEqual(ShamanClientStatus.InRoom, client.Key.GetStatus());
            }
            
            foreach (var client in clients)
            {
                client.Key.SendEvent(new TestEvent() {IntValue = 444});
            }
            
            await Task.Delay(5000);
            
            Assert.AreEqual(10, testEventsReceivedTimes.Count);
            Assert.IsTrue(testEventsReceivedTimes.All(i => i.Value == roomPlayers.Count(r => r.Value == roomPlayers[i.Key])));
            
            Assert.AreEqual(2, rooms.Count);
            Assert.AreNotEqual(Guid.Empty, rooms.First());
        }

        private static string StringPayload = "Very-very long string, which will be passed through unstable UDP transport, Let's see what we will get";
        private static Dictionary<string, string> StringDictionaryPayload = new Dictionary<string, string>
        {
            { "One", "First element of dictionary" },
            { "Two", "Second element of dictionary" },
            { "Three", "Third element of dictionary" },
            { "Four", "Fourth element of dictionary" },
        };
        
        private void OnHeavyTestEventReceived(HeavyTestEvent eve)
        {
            Assert.AreEqual(StringPayload, eve.StringValue);
            Assert.AreEqual(eve.StringDictionary.Count, StringDictionaryPayload.Count);
            foreach (var item in eve.StringDictionary)
            {
                Assert.AreEqual(item.Value, StringDictionaryPayload[item.Key]);
            }
        }
        
        [Test]
        public async Task HeavyLoadTests()
        {
            var clients = new Dictionary<IShamanClientPeer, Guid>();
            var roomPlayers = new Dictionary<IShamanClientPeer, Guid>();
            var testEventsReceivedTimes = new ConcurrentDictionary<IShamanClientPeer, int>();
            var taskScheduler = new TaskScheduler(new ConsoleLogger());
            var clientsCount = 100;
            var eventsCount = 100;
            var mutex = new object();
            for (int i = 0; i < clientsCount; i++)
            {
                clients.Add(_clientFactory.GetClient(), Guid.NewGuid());
            }

            foreach (var client in clients)
            {
                var ping = await client.Key.Ping("127.0.0.1", 23452);
                Assert.AreNotEqual(0, ping);
            }

            foreach (var client in clients)
            {
                var joinInfo = await client.Key.DirectConnectToGameServerToRandomRoom("127.0.0.1", 23452,
                    client.Value, new Dictionary<byte, object>(), new Dictionary<byte, object>());
                roomPlayers[client.Key] = joinInfo.RoomId;
                client.Key.RegisterOperationHandler<HeavyTestEvent>(eve =>
                {
                    OnHeavyTestEventReceived(eve);

                    lock (mutex)
                    {
                        if (!testEventsReceivedTimes.TryAdd(client.Key, 1))
                            testEventsReceivedTimes[client.Key] += 1;
                    }
                });
                // client.Key.RegisterOperationHandler<TestEvent>(eve =>
                // {
                //     OnTestEventReceived(eve);
                //     lock (mutex)
                //     {
                //         if (!testEventsReceivedTimes.TryAdd(client.Key, 1))
                //             testEventsReceivedTimes[client.Key] += 1;
                //     }
                // });
            }

            await Task.Delay(3000);

            foreach (var client in clients)
            {
                Assert.AreEqual(ShamanClientStatus.InRoom, client.Key.GetStatus());
            }
            
            foreach (var client in clients)
            {
                for (var i = 0; i < eventsCount; i++)
                {
                    //lets add some threads
                    taskScheduler.ScheduleOnceOnNow(() => client.Key.SendEvent(new HeavyTestEvent() {StringValue = StringPayload, StringDictionary = StringDictionaryPayload}));
                    // taskScheduler.ScheduleOnceOnNow(() => client.Key.SendEvent(new TestEvent() {IntValue = 444}));
                }
            }
            
            await Task.Delay(300000);
            
            Assert.AreEqual(clientsCount, testEventsReceivedTimes.Count);
            foreach(var item in testEventsReceivedTimes)
                Console.WriteLine($"Player {item.Key}: {item.Value}");
            Assert.IsTrue(testEventsReceivedTimes.All(i => i.Value == eventsCount * roomPlayers.Count(r => r.Value == roomPlayers[i.Key])));
        }
    }
}