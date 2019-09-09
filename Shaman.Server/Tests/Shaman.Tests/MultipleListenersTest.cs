using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game;
using Shaman.Game.Configuration;
using Shaman.Game.Data;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.GameModeControllers;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Tests.ClientPeers;
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Messages.RoomFlow;

namespace Shaman.Tests
{
    [TestFixture]
    public class MultipleListenersTest : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort SERVER_PORT_1 = 23451;
        private const ushort SERVER_PORT_2 = 23452;

        private const ushort WAIT_TIMEOUT = 100;
        
        private GameApplication _gameApplication;
        private TestClientPeer _client1, _client2;
        private Task emptyTask = new Task(() => {});
        private IRequestSender _requestSender;
        private IBackendProvider _backendProvider;
        private IStorageContainer _storageContainer;
        private IRoomPropertiesContainer _roomPropertiesContainer;
        private IRoomManager _roomManager;
        private IGameModeControllerFactory _gameModeControllerFactory;
        private IPacketSender _packetSender;
        
        [SetUp]
        public void Setup()
        {
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            var config =
                new GameApplicationConfig("127.0.0.1", new ushort[] {SERVER_PORT_1, SERVER_PORT_2}, "", "", 7000);
            _requestSender = new FakeSender();

            _backendProvider = new BackendProvider(taskSchedulerFactory, config, _requestSender, _serverLogger);
            //setup server
            _storageContainer = new GameServerStorageContainer(
                _requestSender, 
                new GameServerStorageUpdater(
                    _requestSender,
                    _serverLogger,
                    _backendProvider,
                    serializerFactory),
                _serverLogger,
                serializerFactory,
                taskSchedulerFactory);
            _roomPropertiesContainer = new RoomPropertiesContainer(_serverLogger);
            _gameModeControllerFactory = new FakeGameModeControllerFactory();
            _packetSender = new PacketBatchSender(taskSchedulerFactory, config, serializerFactory);
            _roomManager = new RoomManager(_serverLogger, serializerFactory, config, taskSchedulerFactory, _requestSender, _backendProvider, _gameModeControllerFactory, _roomPropertiesContainer, _storageContainer, _packetSender);
            _gameApplication = new GameApplication(_serverLogger, config, serializerFactory, socketFactory, taskSchedulerFactory, _requestSender, _backendProvider, _storageContainer, _roomManager, _packetSender);
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
                new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.DefaultGameMode}},
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