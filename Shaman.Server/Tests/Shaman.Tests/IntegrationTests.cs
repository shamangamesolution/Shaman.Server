using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game;
using Shaman.Game.Configuration;
using Shaman.Game.Contract;
using Shaman.Game.Metrics;
using Shaman.MM;
using Shaman.MM.Configuration;
using Shaman.MM.MatchMaking;
using Shaman.MM.Players;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.Tests.Providers;
using Shaman.TestTools.ClientPeers;
using GameProject = Shaman.Messages.General.Entity.GameProject;
using IRoomManager = Shaman.Game.Rooms.IRoomManager;
using RoomManager = Shaman.Game.Rooms.RoomManager;

namespace Shaman.Tests
{
    [TestFixture]
    public class IntegrationTests : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort SERVER_PORT_GAME = 23451;
        private const ushort SERVER_PORT_MM = 23452;
        private const ushort WAIT_TIMEOUT = 1000;
        private const int MM_TICK = 250;
        private const int CLIENTS_NUMBER_1 = 12;
        private const int CLIENTS_NUMBER_2 = 100;
        
        private const int TOTAL_PLAYERS_NEEDED_1 = 12;
        private const int EVENTS_SENT = 100;
        
        private GameApplication _gameApplication;
        private MmApplication _mmApplication;

        private IPlayerCollection playerCollection = null; 
        private IMatchMaker matchMaker;// = new MatchMaker();
        private List<MatchMakingGroup> matchMakingGroups = new List<MatchMakingGroup>();
        private IRequestSender requestSender = null;
        private List<TestClientPeer> _clients = new List<TestClientPeer>();
        private IBackendProvider _backendProvider;
        private IRoomManager _roomManager;
        private IGameModeControllerFactory _gameModeControllerFactory;
        private IPacketSender _mmPacketSender, _gamePacketSender;
        private IStatisticsProvider _statsProvider;
        private IMatchMakerServerInfoProvider _serverProvider;
        private IPlayersManager _playerManager;
        private IMatchMakingGroupsManager _mmGroupManager;
        private MM.Managers.IRoomManager _mmRoomManager;
        private IBotManager _botManager;
        private Dictionary<byte, object> _roomProperties = new Dictionary<byte, object>();
        private Dictionary<byte, object> _measures = new Dictionary<byte, object>();
        private Guid CreateRoomDelegate(Dictionary<byte, object> properties)
        {
            return _gameApplication.CreateRoom(properties, new Dictionary<Guid, Dictionary<byte, object>>());
        }

        private void UpdateRoomDelegate(Guid roomId)
        {
            _gameApplication.UpdateRoom(roomId, new Dictionary<Guid, Dictionary<byte, object>>());
        }
        
        [SetUp]
        public void Setup()
        {             
            _clients.Clear();
            var config = new MmApplicationConfig("", "127.0.0.1", new List<ushort> {SERVER_PORT_MM}, "", 120000, 120000, GameProject.DefaultGame, "");
            playerCollection = new PlayerCollection(_serverLogger, Mock.Of<IMmMetrics>());
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            //fake sender to direct calls of application functions
            requestSender = new FakeSenderWithGameApplication(new Dictionary<byte, object> {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.SinglePlayer}}, CreateRoomDelegate,  UpdateRoomDelegate);
            _backendProvider = new BackendProvider(taskSchedulerFactory, config, requestSender, _serverLogger);
            var gameConfig = new GameApplicationConfig(
                "","",
                "127.0.0.1",
                new List<ushort> {SERVER_PORT_GAME},
                "",
                "",
                7000);
            _mmPacketSender = new PacketBatchSender(taskSchedulerFactory, config, serializerFactory);
            _gamePacketSender = new PacketBatchSender(taskSchedulerFactory, gameConfig, serializerFactory);

            var createdRoomManager = new CreatedRoomManager(taskSchedulerFactory, _serverLogger);
            _statsProvider = new MM.Providers.StatisticsProvider(playerCollection);
            //_serverProvider = new MatchMakerServerInfoProvider(requestSender, taskSchedulerFactory, config, _serverLogger, _statsProvider);
            _serverProvider = new FakeMatchMakerServerInfoProvider(requestSender, "127.0.0.1", $"{SERVER_PORT_GAME}");
            _playerManager = new PlayersManager( Mock.Of<IMmMetrics>(), _serverLogger);
            _mmRoomManager =
                new MM.Managers.RoomManager(_serverProvider, _serverLogger, taskSchedulerFactory.GetTaskScheduler());
            _botManager = new BotManager();

            _mmGroupManager = new MatchMakingGroupManager(_serverLogger, taskSchedulerFactory, _playerManager, _mmPacketSender,  Mock.Of<IMmMetrics>(), _serverProvider, _mmRoomManager, _botManager);
            
            matchMaker = new MatchMaker(playerCollection, _serverLogger, _mmPacketSender, Mock.Of<IMmMetrics>(), createdRoomManager, _playerManager,_mmGroupManager);
            
            _roomProperties = new Dictionary<byte, object>();
            _roomProperties.Add(PropertyCode.RoomProperties.MatchMakingTick, MM_TICK);
            _roomProperties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded, TOTAL_PLAYERS_NEEDED_1);
            _roomProperties.Add(PropertyCode.RoomProperties.ToAddBots, false);
            _roomProperties.Add(PropertyCode.RoomProperties.ToAddOtherPlayers, true);
            _roomProperties.Add(PropertyCode.RoomProperties.TimeBeforeBotsAdded, 5000);
            _roomProperties.Add(PropertyCode.RoomProperties.RoomIsClosingIn, 120000);
            _measures = new Dictionary<byte, object>();
            _measures.Add(PropertyCode.PlayerProperties.Level, 1);
            //matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_1, MM_TICK, false, true, 5000, 120000, new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.SinglePlayer}}, new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 1}});
            matchMaker.AddMatchMakingGroup(_roomProperties, _measures);
            
            matchMaker.AddRequiredProperty(PropertyCode.PlayerProperties.Level);
            
            //setup mm server
            _mmApplication = new MmApplication(_serverLogger, config, serializerFactory, socketFactory, playerCollection, matchMaker,requestSender, taskSchedulerFactory, _backendProvider, _mmPacketSender, createdRoomManager, _serverProvider, _mmRoomManager);
            
            _mmApplication.Start();

            _gameModeControllerFactory = new FakeGameModeControllerFactory();

            _roomManager = new RoomManager(_serverLogger, serializerFactory, gameConfig, taskSchedulerFactory, _gameModeControllerFactory, _mmPacketSender, Mock.Of<IGameMetrics>());

            
            //setup game server
            _gameApplication = new GameApplication(
                _serverLogger, 
                gameConfig, 
                serializerFactory, 
                socketFactory, 
                taskSchedulerFactory, 
                requestSender, 
                _backendProvider,
                _roomManager,
                _gamePacketSender);
            
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
                EmptyTask.Wait(WAIT_TIMEOUT);
            }
            
            //send auth
            _clients.ForEach(c => c.Send(new AuthorizationRequest(1, Guid.NewGuid())));
            EmptyTask.Wait(WAIT_TIMEOUT * CLIENTS_NUMBER_1);
            
            //check auth success
            isSuccess = true;
            _clients.ForEach(c =>
            {
                var countOfSuccessResponses = c.GetCountOfSuccessResponses(CustomOperationCode.Authorization);
                if (countOfSuccessResponses == 0)
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
                EmptyTask.Wait(WAIT_TIMEOUT);
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