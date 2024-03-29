using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Game;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.RoomFlow;
using Shaman.Tests.Helpers;
using Shaman.TestTools.ClientPeers;

namespace Shaman.Tests
{
    [TestFixture]
    public class MultipleListenersTest : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort SERVER_PORT_1 = 23451;
        private const ushort SERVER_PORT_2 = 23452;

        private const ushort WAIT_TIMEOUT = 300;
        
        private GameApplication _gameApplication;
        private TestClientPeer _client1, _client2;
        [SetUp]
        public void Setup()
        {
            _gameApplication = InstanceHelper.GetGame(new List<ushort> {SERVER_PORT_1, SERVER_PORT_2});
            _gameApplication.Start();
            
            //setup client
            _client1 = new TestClientPeer(_clientLogger, taskSchedulerFactory, serializer);
            _client2 = new TestClientPeer(_clientLogger, taskSchedulerFactory, serializer);        

        }

        [TearDown]
        public void TearDown()
        {
            _gameApplication.ShutDown();
        }
        
        [Test]
        public async Task TwoClientsTest()
        {
            var emptyTask = new Task(() => {
                
            });
            //check stats
            var stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);
            Assert.AreEqual(0, stats.RoomCount);
            
            var roomId = _gameApplication.CreateRoom(
                new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.SinglePlayer}},
                new Dictionary<Guid, Dictionary<byte, object>>());
            
            _serverLogger.Info($"Room {roomId} created");
            _client1.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT_1);
            _client2.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT_2);
            emptyTask.Wait(WAIT_TIMEOUT);

            stats = _gameApplication.GetStats();
            Assert.AreEqual(2, stats.PeerCount);
            Assert.AreEqual(1, stats.RoomCount);
            await _client1.Send<AuthorizationResponse>(new AuthorizationRequest() {SessionId = Guid.NewGuid()});            
            await _client2.Send<AuthorizationResponse>(new AuthorizationRequest() {SessionId = Guid.NewGuid()});         
            
            await _client1.Send<JoinRoomResponse>(new JoinRoomRequest(roomId, new Dictionary<byte, object>()));
            await _client2.Send<JoinRoomResponse>(new JoinRoomRequest(roomId, new Dictionary<byte, object>()));

            stats = _gameApplication.GetStats();
            Assert.AreEqual(2, stats.PeerCount);
            Assert.AreEqual(1, stats.RoomCount);
            var roomPeerCount = stats.RoomsPeerCount;
            Assert.AreEqual(2, roomPeerCount[roomId]);

        }
    }
}