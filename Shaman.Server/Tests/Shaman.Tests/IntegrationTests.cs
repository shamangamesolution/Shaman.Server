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
using Shaman.MM.Contract;
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
        private IRoomPropertiesProvider _roomPropertiesProvider;

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
            var config = new MmApplicationConfig("", "127.0.0.1", new List<ushort> {SERVER_PORT_MM}, "", 120000, GameProject.DefaultGame, "", 7002);
            _roomPropertiesProvider = new FakeRoomPropertiesProvider3();
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
            _mmPacketSender = new PacketBatchSender(taskSchedulerFactory, config, serializer);
            _gamePacketSender = new PacketBatchSender(taskSchedulerFactory, gameConfig, serializer);
            
            _playerManager = new PlayersManager( Mock.Of<IMmMetrics>(), _serverLogger);
            _statsProvider = new MM.Providers.StatisticsProvider(_playerManager);
            //_serverProvider = new MatchMakerServerInfoProvider(requestSender, taskSchedulerFactory, config, _serverLogger, _statsProvider);
            _serverProvider = new FakeMatchMakerServerInfoProvider(requestSender, "127.0.0.1", $"{SERVER_PORT_GAME}");
            _mmRoomManager =
                new MM.Managers.RoomManager(_serverProvider, _serverLogger, taskSchedulerFactory);

            _mmGroupManager = new MatchMakingGroupManager(_serverLogger, taskSchedulerFactory, _playerManager, _mmPacketSender,  Mock.Of<IMmMetrics>(),  _mmRoomManager, _roomPropertiesProvider, config);
            
            matchMaker = new MatchMaker(_serverLogger, _mmPacketSender, Mock.Of<IMmMetrics>(), _playerManager,_mmGroupManager);

            _measures = new Dictionary<byte, object>();
            _measures.Add(PropertyCode.PlayerProperties.Level, 1);
            //matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_1, MM_TICK, false, true, 5000, 120000, new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.SinglePlayer}}, new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 1}});
            matchMaker.AddMatchMakingGroup(_measures);
            
            matchMaker.AddRequiredProperty(PropertyCode.PlayerProperties.Level);
            
            //setup mm server
            _mmApplication = new MmApplication(_serverLogger, config, serializer, socketFactory, matchMaker,requestSender, taskSchedulerFactory, _backendProvider, _mmPacketSender, _serverProvider, _mmRoomManager, _mmGroupManager, _playerManager);
            
            _mmApplication.Start();

            _gameModeControllerFactory = new FakeGameModeControllerFactory();

            _roomManager = new RoomManager(_serverLogger, serializer, gameConfig, taskSchedulerFactory, _gameModeControllerFactory, _mmPacketSender, Mock.Of<IGameMetrics>(), requestSender);

            
            //setup game server
            _gameApplication = new GameApplication(
                _serverLogger, 
                gameConfig, 
                serializer, 
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
                var client = new TestClientPeer( _clientLogger, taskSchedulerFactory, serializer);
                client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM);
                _clients.Add(client);
                EmptyTask.Wait(WAIT_TIMEOUT);
            }
            
            //send auth
            _clients.ForEach(c => c.Send<AuthorizationResponse>(new AuthorizationRequest(1, Guid.NewGuid())).Wait());

            //send join matchmaking (with level = 1)
            _clients.ForEach(c => c.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 1} })).Wait());
            _clients.ForEach(c=>c.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady));

            var mmStats = _mmApplication.GetStats();
            Assert.AreEqual(1, mmStats.RegisteredServers.Count);
            
            //sending leave mathmaking request
            _clients.ForEach(c => c.Send<LeaveMatchMakingResponse>(new LeaveMatchMakingRequest()).Wait());
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
                if (c.CountOf<TestRoomEvent>() != (CLIENTS_NUMBER_1 - 1) * EVENTS_SENT)
                {
                    _clientLogger.Error($"test events {c.CountOf<TestRoomEvent>()}/{(CLIENTS_NUMBER_1 - 1) * EVENTS_SENT}");
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
                var client = new TestClientPeer(_clientLogger, taskSchedulerFactory, serializer);
                client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM);
                _clients.Add(client);
                EmptyTask.Wait(WAIT_TIMEOUT);
            }
            
            //send auth
            _clients.ForEach(c => c.Send<AuthorizationResponse>(new AuthorizationRequest(1, Guid.NewGuid())).Wait());
            
            //send join matchmaking (with level = 1)
            _clients.ForEach(c => c.Send(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 1} })));
            
            EmptyTask.Wait(MM_TICK* (CLIENTS_NUMBER_2 / 8));
            //wait maximum mm time
            EmptyTask.Wait(6000);

            //check joininfo existance
            isSuccess = true;
            int notJoinedCount = 0;
            int joinedCount = 0;
            _clients.ForEach(c =>
            {
                if (c.GetJoinInfo() == null || c.GetJoinInfo().Status != JoinStatus.RoomIsReady)
                {
                    if (c.GetJoinInfo() != null)
                        _clientLogger.Info($"Checking joinInfo. Status = {c.GetJoinInfo().Status}");
                    notJoinedCount++;
                    isSuccess = false;
                }
                else
                {
                    if (c.GetJoinInfo() != null)
                        _clientLogger.Info($"Checking joinInfo. Status = {c.GetJoinInfo().Status}");
                    joinedCount++;
                }
            });
            var roomsCount = CLIENTS_NUMBER_2 / TOTAL_PLAYERS_NEEDED_1 + (CLIENTS_NUMBER_2 % TOTAL_PLAYERS_NEEDED_1 > 0 ? 1: 0);
            
            Assert.AreEqual(CLIENTS_NUMBER_2 - joinedCount, notJoinedCount);
            
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
            Assert.AreEqual(roomsCount, stats.RoomCount);
            
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