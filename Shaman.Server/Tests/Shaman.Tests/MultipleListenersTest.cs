using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game;
using Shaman.Game.Configuration;
using Shaman.Game.Contract;
using Shaman.Game.Metrics;
using Shaman.Game.Providers;
using Shaman.Game.Rooms;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.RoomFlow;
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
        private IRequestSender _requestSender;
        private IBackendProvider _backendProvider;
        private IRoomManager _roomManager;
        private IGameModeControllerFactory _gameModeControllerFactory;
        private IPacketSender _packetSender;
        [SetUp]
        public void Setup()
        {
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            var config =
                new GameApplicationConfig("", "", "127.0.0.1", new List<ushort> {SERVER_PORT_1, SERVER_PORT_2}, "", "", 7000);
            _requestSender = new FakeSender();
            _backendProvider = new BackendProvider(taskSchedulerFactory, config, _requestSender, _serverLogger);
            //setup server
            _gameModeControllerFactory = new FakeGameModeControllerFactory();
            _packetSender = new PacketBatchSender(taskSchedulerFactory, config, serializerFactory);
            _roomManager = new RoomManager(_serverLogger, serializerFactory, config, taskSchedulerFactory,  _gameModeControllerFactory, _packetSender,Mock.Of<IGameMetrics>(), _requestSender);
            _gameApplication = new GameApplication(_serverLogger, config, serializerFactory, socketFactory, taskSchedulerFactory, _requestSender, _backendProvider, _roomManager, _packetSender);
            _gameApplication.Start();
            
            //setup client
            _client1 = new TestClientPeer(_clientLogger, taskSchedulerFactory);
            _client2 = new TestClientPeer(_clientLogger, taskSchedulerFactory);        

        }

        [TearDown]
        public void TearDown()
        {
            _gameApplication.ShutDown();
        }
        
        [Test]
        public void TwoClientsTest()
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
            _client1.Send(new AuthorizationRequest(1, Guid.NewGuid()));            
            _client2.Send(new AuthorizationRequest(1, Guid.NewGuid()));            
            emptyTask.Wait(WAIT_TIMEOUT);
            _client1.Send(new JoinRoomRequest(roomId, new Dictionary<byte, object>()));
            _client2.Send(new JoinRoomRequest(roomId, new Dictionary<byte, object>()));
            emptyTask.Wait(WAIT_TIMEOUT);

            Assert.AreEqual(2, _client1.GetCountOfSuccessResponses(CustomOperationCode.Authorization) + _client2.GetCountOfSuccessResponses(CustomOperationCode.Authorization));
            Assert.AreEqual(2, _client1.GetCountOfSuccessResponses(CustomOperationCode.JoinRoom) + _client2.GetCountOfSuccessResponses(CustomOperationCode.JoinRoom));

            stats = _gameApplication.GetStats();
            Assert.AreEqual(2, stats.PeerCount);
            Assert.AreEqual(1, stats.RoomCount);
            var roomPeerCount = stats.RoomsPeerCount;
            Assert.AreEqual(2, roomPeerCount[roomId]);

        }
    }
}