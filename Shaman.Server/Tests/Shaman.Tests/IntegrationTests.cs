using System;
using System.Collections.Generic;
using System.Linq;
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
using Shaman.MM;
using Shaman.MM.Configuration;
using Shaman.MM.MatchMaking;
using Shaman.MM.Players;
using Shaman.MM.Servers;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Tests.ClientPeers;
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Shaman.Tests
{
    [TestFixture]
    public class IntegrationTests : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort SERVER_PORT_GAME = 23451;
        private const ushort SERVER_PORT_MM = 23452;
        private const ushort WAIT_TIMEOUT = 100;
        private const ushort MM_TICK = 1000;
        private const ushort CLIENTS_NUMBER_1 = 12;
        private const ushort CLIENTS_NUMBER_2 = 100;
        
        private const ushort TOTAL_PLAYERS_NEEDED_1 = 12;
        private const ushort EVENTS_SENT = 100;
        
        private GameApplication _gameApplication;
        private MmApplication _mmApplication;

        private IRegisteredServerCollection serverCollection = null; 
        private IPlayerCollection playerCollection = null; 
        private IMatchMaker matchMaker;// = new MatchMaker();
        private List<MatchMakingGroup> matchMakingGroups = new List<MatchMakingGroup>();
        private IRequestSender requestSender = null;
        private List<TestClientPeer> _clients = new List<TestClientPeer>();
        private IBackendProvider _backendProvider;
        private IStorageContainer _storageContainer;
        private IRoomPropertiesContainer _roomPropertiesContainer;
        private IRoomManager _roomManager;
        private IGameModeControllerFactory _gameModeControllerFactory;
        private IPacketSender _packetSender;
        private Guid CreateRoomDelegate(Dictionary<byte, object> properties)
        {
            return _gameApplication.CreateRoom(properties, new Dictionary<Guid, Dictionary<byte, object>>());
        }

        private void ActualizeServerDelegate(ActualizeServerRequest request)
        {
            _mmApplication.ActualizeServer(request);
        }
        
        [SetUp]
        public void Setup()
        {             
            _clients.Clear();
            var config = new MmApplicationConfig("127.0.0.1", new ushort[] {SERVER_PORT_MM}, "", 120000, 120000, GameProject.DefaultGame, "", "");
            playerCollection = new PlayerCollection(_serverLogger, serializerFactory);
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            serverCollection = new RegisteredServersCollection(_serverLogger, config, taskSchedulerFactory);
            //fake sender to direct calls of application functions
            requestSender = new FakeSenderWithGameApplication(new Dictionary<byte, object> {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.DefaultGameMode}}, CreateRoomDelegate, ActualizeServerDelegate);
            _backendProvider = new BackendProvider(taskSchedulerFactory, config, requestSender, _serverLogger);
            
            var gameConfig = new GameApplicationConfig(
                "127.0.0.1",
                new ushort[] {SERVER_PORT_GAME},
                "",
                "",
                7000);
            _packetSender = new PacketBatchSender(taskSchedulerFactory, gameConfig, serializerFactory);
            
            matchMaker = new MatchMaker(serverCollection, playerCollection, _serverLogger, taskSchedulerFactory, serializerFactory, _packetSender);
            matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_1, MM_TICK, false, true, 5000, new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.DefaultGameMode}}, new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 1}});
            
            //setup mm server
            _mmApplication = new MmApplication(_serverLogger, config, serializerFactory, socketFactory, serverCollection, playerCollection, matchMaker,requestSender, taskSchedulerFactory, _backendProvider, _packetSender);
            _mmApplication.SetMatchMakerProperties(new List<byte> {PropertyCode.PlayerProperties.Level});
            
            _mmApplication.Start();
            
            _storageContainer = new GameServerStorageContainer(
                requestSender, 
                new GameServerStorageUpdater(
                    requestSender,
                    _serverLogger,
                    _backendProvider,
                    serializerFactory),
                _serverLogger,
                serializerFactory,
                taskSchedulerFactory);
            
            _roomPropertiesContainer = new RoomPropertiesContainer(_serverLogger);
            _gameModeControllerFactory = new FakeGameModeControllerFactory();

            _roomManager = new RoomManager(_serverLogger, serializerFactory, gameConfig, taskSchedulerFactory, requestSender, _backendProvider, _gameModeControllerFactory, _roomPropertiesContainer, _storageContainer, _packetSender);

            
            //setup game server
            _gameApplication = new GameApplication(
                _serverLogger, 
                gameConfig, 
                serializerFactory, 
                socketFactory, 
                taskSchedulerFactory, 
                requestSender, 
                _backendProvider,
                _storageContainer,
                _roomManager,
                _packetSender);
            
            _gameApplication.Start();
            
            
        }
        
        [TearDown]
        public void TearDown()
        {
            _mmApplication.ShutDown();
            _gameApplication.ShutDown();
        }

        [Test]
        public void IntegrationTest()
        {
            var isSuccess = false;
            //create 12 clients and connect them to mm
            for (var i = 0; i < CLIENTS_NUMBER_1; i++)
            {
                var client = new TestClientPeer( _clientLogger, taskSchedulerFactory);
                client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM);
                _clients.Add(client);
            }
            
            //send auth
            _clients.ForEach(c => c.Send(new AuthorizationRequest(1, Guid.NewGuid())));
            EmptyTask.Wait(WAIT_TIMEOUT);
            //check auth success
            isSuccess = true;
            _clients.ForEach(c =>
            {
                if (c.GetCountOfSuccessResponses(CustomOperationCode.Authorization) == 0)
                    isSuccess = false;
            });
            Assert.IsTrue(isSuccess);
            
            //send join matchmaking (with level = 1)
            _clients.ForEach(c => c.Send(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 1} })));
            
            EmptyTask.Wait(MM_TICK*2);

            //check joininfo existance
            isSuccess = true;
            _clients.ForEach(c =>
            {
                if (c.GetJoinInfo() == null || c.GetJoinInfo().Status != JoinStatus.RoomIsReady)
                    isSuccess = false;
            });
            Assert.IsTrue(isSuccess);

            var mmStats = _mmApplication.GetStats();
            Assert.AreEqual(1, mmStats.RegisteredServers.Count);
            
            //sending leave mathmaking request
            _clients.ForEach(c => c.Send(new LeaveMatchMakingRequest()));
            EmptyTask.Wait(WAIT_TIMEOUT);
            _clients.ForEach(c => c.Disconnect());

            mmStats = _mmApplication.GetStats();
            Assert.AreEqual(0, mmStats.TotalPlayers);
            Assert.AreEqual(1, mmStats.RegisteredServers.Count);
            
            //connect to server
            _clients.ForEach(c => c.Connect(c.GetJoinInfo().ServerIpAddress.ToString(), c.GetJoinInfo().ServerPort));
            var roomId = _clients.First().GetJoinInfo().RoomId;
            EmptyTask.Wait(MM_TICK*2);
            var stats = _gameApplication.GetStats();
            Assert.AreEqual(CLIENTS_NUMBER_1, stats.PeerCount);
            Assert.AreEqual(1, stats.RoomCount);
            var roomsPeerCount = stats.RoomsPeerCount.First();
            //players did not join room yet
            Assert.AreEqual(roomId, roomsPeerCount.Key);
            Assert.AreEqual(0, roomsPeerCount.Value);
            
            //authing
            _clients.ForEach(c => c.Send(new AuthorizationRequest(1, Guid.NewGuid())));
            EmptyTask.Wait(WAIT_TIMEOUT);
            
            //joining room
            _clients.ForEach(c => c.Send(new JoinRoomRequest(roomId, new Dictionary<byte, object>())));
            EmptyTask.Wait(WAIT_TIMEOUT);
            stats = _gameApplication.GetStats();
            roomsPeerCount = stats.RoomsPeerCount.First();
            
            //players joined room
            Assert.AreEqual(roomId, roomsPeerCount.Key);
            Assert.AreEqual(CLIENTS_NUMBER_1, roomsPeerCount.Value);

            //activity
            //each client sends 1000 events to others
            _clients.ForEach(c =>
            {
                for (int i = 0; i < EVENTS_SENT; i++)
                {
                    c.Send(new TestRoomEvent(true, 122, 4.668f, new List<int>()));
                }
            });
            
            EmptyTask.Wait(WAIT_TIMEOUT * 100);

            isSuccess = true;
            _clients.ForEach(c =>
            {
                if (c.GetCountOf(CustomOperationCode.Test) != (CLIENTS_NUMBER_1 - 1) * EVENTS_SENT)
                {
                    _clientLogger.Error($"test events {c.GetCountOf(CustomOperationCode.Test)}/{(CLIENTS_NUMBER_1 - 1) * EVENTS_SENT}");
                    isSuccess = false;
                }
            });
            Assert.IsTrue(isSuccess);

            //leave room
            _clients.ForEach(c => c.Send(new LeaveRoomEvent()));
            EmptyTask.Wait(WAIT_TIMEOUT);
            stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.RoomsPeerCount.Count);
            Assert.AreEqual(CLIENTS_NUMBER_1, stats.PeerCount);
            
            //disconnect from server
            _clients.ForEach(c => c.Disconnect());
            EmptyTask.Wait(WAIT_TIMEOUT);
            stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);
            
        }

        [Test]
        public void LotsOfClientsGoToGameServer()
        {
            var isSuccess = false;
            //create N clients and connect them to mm
            for (var i = 0; i < CLIENTS_NUMBER_2; i++)
            {
                var client = new TestClientPeer(_clientLogger, taskSchedulerFactory);
                client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM);
                _clients.Add(client);
            }
            
            //send auth
            _clients.ForEach(c => c.Send(new AuthorizationRequest(1, Guid.NewGuid())));
            EmptyTask.Wait(WAIT_TIMEOUT * 10);
            
            //check auth success
            isSuccess = true;
            _clients.ForEach(c =>
            {
                if (c.GetCountOfSuccessResponses(CustomOperationCode.Authorization) == 0)
                    isSuccess = false;
            });
            Assert.IsTrue(isSuccess);
            
            //send join matchmaking (with level = 1)
            _clients.ForEach(c => c.Send(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 1} })));
            
            EmptyTask.Wait(MM_TICK* (CLIENTS_NUMBER_2 / 8));
            
            //check joininfo existance
            isSuccess = true;
            int notJoinedCount = 0;
            int joinedCount = 0;
            _clients.ForEach(c =>
            {
                if (c.GetJoinInfo() == null || c.GetJoinInfo().Status != JoinStatus.RoomIsReady)
                {
                    notJoinedCount++;
                    isSuccess = false;
                }
                else
                {
                    joinedCount++;
                }
            });
            var roomsCount = CLIENTS_NUMBER_2 / TOTAL_PLAYERS_NEEDED_1;
            var totalPlayersJoined = roomsCount * TOTAL_PLAYERS_NEEDED_1;
            
            Assert.AreEqual(totalPlayersJoined, joinedCount);
            Assert.AreEqual(CLIENTS_NUMBER_2 - totalPlayersJoined, notJoinedCount);
            
            var mmStats = _mmApplication.GetStats();
            Assert.AreEqual(1, mmStats.RegisteredServers.Count);
            
            //sending leave mathmaking request
            _clients.ForEach(c => c.Send(new LeaveMatchMakingRequest()));
            EmptyTask.Wait(WAIT_TIMEOUT * 10);
            _clients.ForEach(c => c.Disconnect());

            mmStats = _mmApplication.GetStats();
            Assert.AreEqual(0, mmStats.TotalPlayers);
            Assert.AreEqual(1, mmStats.RegisteredServers.Count);
            
            //connect to server
            _clients.Where(c => c.GetJoinInfo() != null && c.GetJoinInfo().Status == JoinStatus.RoomIsReady).ToList().ForEach(c => c.Connect(c.GetJoinInfo().ServerIpAddress.ToString(), c.GetJoinInfo().ServerPort));
            EmptyTask.Wait(MM_TICK*20);
            var stats = _gameApplication.GetStats();
            Assert.AreEqual(joinedCount, stats.PeerCount);
            Assert.AreEqual(CLIENTS_NUMBER_2 / TOTAL_PLAYERS_NEEDED_1, stats.RoomCount);
            
            //authing
            _clients.ForEach(c => c.Send(new AuthorizationRequest(1, Guid.NewGuid())));
            EmptyTask.Wait(WAIT_TIMEOUT);
            
            //joining room
            _clients.Where(c => c.GetJoinInfo() != null).ToList().ForEach(c => c.Send(new JoinRoomRequest(c.GetJoinInfo().RoomId, new Dictionary<byte, object>())));
            EmptyTask.Wait(WAIT_TIMEOUT * 100);
            stats = _gameApplication.GetStats();
            Assert.AreEqual(roomsCount, stats.RoomCount);
            Assert.AreEqual(stats.RoomsPeerCount.Count, stats.RoomCount);
            
            //leave room
//            _clients.ForEach(c => c.Send(new LeaveRoomEvent()));
//            EmptyTask.Wait(WAIT_TIMEOUT * 300);
//            
//            stats = _gameApplication.GetStats();
//            Assert.AreEqual(0, stats.RoomCount);
//            Assert.AreEqual(totalPlayersJoined, stats.PeerCount);

            //disconnect from server
            _clients.ForEach(c => c.Disconnect());
            EmptyTask.Wait(WAIT_TIMEOUT);
            stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);
        }
    }
}