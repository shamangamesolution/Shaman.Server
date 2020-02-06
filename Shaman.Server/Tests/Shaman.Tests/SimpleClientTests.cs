using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shaman.Client;
using Shaman.Client.Peers;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game;
using Shaman.Game.Configuration;
using Shaman.Game.Contract;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.MM;
using Shaman.MM.Configuration;
using Shaman.MM.MatchMaking;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
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
    public class SimpleClientTests : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort SERVER_PORT_GAME = 23451;
        private const ushort SERVER_PORT_MM = 23452;
        private const ushort WAIT_TIMEOUT = 100;
        private const int MM_TICK = 1000;
        private const int CLIENTS_NUMBER_1 = 24;
        private const int CLIENTS_NUMBER_2 = 1500;
        
        private const int TOTAL_PLAYERS_NEEDED_1 = 6;
        private const int EVENTS_SENT = 100;
        
        private GameApplication _gameApplication;
        private MmApplication _mmApplication;

        private IMatchMaker matchMaker;
        private List<MatchMakingGroup> matchMakingGroups = new List<MatchMakingGroup>();
        private IRequestSender requestSender = null;
        private List<ShamanClientPeerLegacy> _clients = new List<ShamanClientPeerLegacy>();
        private IBackendProvider _backendProvider;
        private IRoomPropertiesContainer _roomPropertiesContainer;
        private IRoomManager _roomManager;
        private IGameModeControllerFactory _gameModeControllerFactory;
        private IPacketSender _mmPacketSender, _gamePacketSender;
        private IStatisticsProvider _statsProvider;
        private IMatchMakerServerInfoProvider _serverProvider;
        private IPlayersManager _playerManager;
        private IMatchMakingGroupsManager _mmGroupManager;
        private IRoomPropertiesProvider _roomPropertiesProvider;

        private MM.Managers.IRoomManager _mmRoomManager;
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
            var config = new MmApplicationConfig("", "127.0.0.1", new List<ushort> {SERVER_PORT_MM}, "", 120000, GameProject.DefaultGame,"", 7002, isAuthOn:false);
            _roomPropertiesProvider = new FakeRoomPropertiesProvider2();
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            //fake sender to direct calls of application functions
            requestSender = new FakeSenderWithGameApplication(new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte)GameMode.SinglePlayer}}, CreateRoomDelegate, UpdateRoomDelegate);
            _backendProvider = new BackendProvider(taskSchedulerFactory, config, requestSender, _serverLogger);
            var gameConfig = new GameApplicationConfig("", "", "127.0.0.1", new List<ushort> {SERVER_PORT_GAME}, "", "", 7000,
                isAuthOn: false);
            _mmPacketSender = new PacketBatchSender(taskSchedulerFactory, config, serializer);
            _gamePacketSender = new PacketBatchSender(taskSchedulerFactory, gameConfig, serializer);
            
            _playerManager = new PlayersManager( Mock.Of<IMmMetrics>(), _serverLogger);
            _statsProvider = new MM.Providers.StatisticsProvider(_playerManager);
            _serverProvider = new FakeMatchMakerServerInfoProvider(requestSender, "127.0.0.1", $"{SERVER_PORT_GAME}");

            _mmRoomManager =
                new MM.Managers.RoomManager(_serverProvider, _serverLogger, taskSchedulerFactory);
            _mmGroupManager = new MatchMakingGroupManager(_serverLogger, taskSchedulerFactory, _playerManager, _mmPacketSender,  Mock.Of<IMmMetrics>(), _mmRoomManager, _roomPropertiesProvider, config);
            matchMaker = new MatchMaker(_serverLogger,  _mmPacketSender, Mock.Of<IMmMetrics>(),  _playerManager, _mmGroupManager);

            _measures = new Dictionary<byte, object>();
            _measures.Add(PropertyCode.PlayerProperties.Level, 1);
            matchMaker.AddMatchMakingGroup(_measures);

            _measures = new Dictionary<byte, object>();
            _measures.Add(PropertyCode.PlayerProperties.Level, 2);
            matchMaker.AddMatchMakingGroup(_measures);
            
            //setup mm server
            _mmApplication = new MmApplication(_serverLogger, config, serializer, socketFactory,  matchMaker,requestSender, taskSchedulerFactory, _backendProvider, _mmPacketSender, _serverProvider, _mmRoomManager, _mmGroupManager, _playerManager);
            matchMaker.AddRequiredProperty(PropertyCode.PlayerProperties.Level);
            
            _mmApplication.Start();
            
            //setup game server
            _roomPropertiesContainer = new RoomPropertiesContainer(_serverLogger);
            _gameModeControllerFactory = new FakeGameModeControllerFactory();

            _roomManager = new RoomManager(_serverLogger, serializer, gameConfig, taskSchedulerFactory,
                _gameModeControllerFactory, _mmPacketSender, Mock.Of<IGameMetrics>(), requestSender);

            _gameApplication = new GameApplication(_serverLogger, gameConfig, serializer, socketFactory, taskSchedulerFactory, requestSender, _backendProvider, _roomManager, _gamePacketSender);
            _gameApplication.Start();
        }
        
        [TearDown]
        public void TearDown()
        {
            _mmApplication.ShutDown();
            _gameApplication.ShutDown();
        }
        
        private ConcurrentDictionary<Guid, int> _eventsCount = new ConcurrentDictionary<Guid, int>();

        class TestMessageDeserializer: IMessageDeserializer
        {
            public MessageBase DeserializeMessage(ushort operationCode, ISerializer serializer, byte[] message)
            {
                return MessageFactory.DeserializeMessageForTest(operationCode, message, 0, message.Length);
            }

            public MessageBase DeserializeMessage(ushort operationCode, ISerializer serializer, byte[] message, int offset, int length)
            {
                return MessageFactory.DeserializeMessageForTest(operationCode, message, offset, length);
            }
        }
        
        [Test]
        public void FullCycleTest()
        {
            for (int i = 0; i < CLIENTS_NUMBER_1; i++)
            {
                var client = new ShamanClientPeerLegacy(new TestMessageDeserializer(), _clientLogger, taskSchedulerFactory, 20, serializer, requestSender);
                var sessionId = Guid.NewGuid();
                client.JoinGame(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM,1, sessionId, new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 1} },
                    new Dictionary<byte, object>(), 
                    (status, joinInfo) =>
                    {
                        if (joinInfo == null)
                            _clientLogger.Info($"Client status changed {status.Status}, isSuccess = {status.IsSuccess}, error = {status.Error}, joinInfo = null");
                        else
                        {
                            _clientLogger.Info($"Client status changed {status.Status}, isSuccess = {status.IsSuccess}, error = {status.Error}, joinInfo.JoinStatus = {joinInfo.Status}, joinInfo.CurrentPlayers = {joinInfo.CurrentPlayers}, joinInfo.MaxPlayers = {joinInfo.MaxPlayers}");
                        }
                    });
                client.RegisterOperationHandler(CustomOperationCode.Test, message =>
                {
                    if (!_eventsCount.ContainsKey(sessionId))
                        _eventsCount.TryAdd(sessionId, 0);
                    _eventsCount[sessionId]++;
                });
                EmptyTask.Wait(TimeSpan.FromSeconds(1));
                _clients.Add(client);
            }
            
            EmptyTask.Wait(TimeSpan.FromSeconds(10));

            bool success = true;
            foreach(var client in _clients)
                if (client.GetStatus() != ClientStatusLegacy.InRoom)
                    success = false;
            Assert.AreEqual(true, success);

            _clients.ForEach(c =>
            {
                for (int i = 0; i < EVENTS_SENT; i++)
                {
                    c.SendEvent(new TestRoomEvent(true, 122, 4.668f, new List<int>()));
                }
            });
            
            EmptyTask.Wait(WAIT_TIMEOUT * 100);

            success = true;
            foreach(var client in _clients)
                if (client.GetStatus() != ClientStatusLegacy.InRoom)
                    success = false;
            
            Assert.AreEqual(true, success);

            foreach (var item in _eventsCount)
            {
                Assert.AreEqual(item.Value, EVENTS_SENT * (TOTAL_PLAYERS_NEEDED_1 - 1));
            }
        }

        [Test]
        public void TestDirectJoin()
        {
            var client = new ShamanClientPeerLegacy(new TestMessageDeserializer(), _clientLogger, taskSchedulerFactory, 20, serializer, requestSender);
            var sessionId = Guid.NewGuid();
            client.JoinGame(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM,1, sessionId, new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 2} },
                new Dictionary<byte, object>(), 
                (status, joinInfo) =>
                {
                    if (joinInfo == null)
                        _clientLogger.Info($"Client status changed {status.Status}, isSuccess = {status.IsSuccess}, error = {status.Error}, joinInfo = null");
                    else
                    {
                        _clientLogger.Info($"Client status changed {status.Status}, isSuccess = {status.IsSuccess}, error = {status.Error}, joinInfo.JoinStatus = {joinInfo.Status}, joinInfo.CurrentPlayers = {joinInfo.CurrentPlayers}, joinInfo.MaxPlayers = {joinInfo.MaxPlayers}");
                    }
                });
            EmptyTask.Wait(TimeSpan.FromSeconds(3));
            Assert.AreEqual(ClientStatusLegacy.InRoom, client.GetStatus());
            Assert.AreEqual(1, _mmRoomManager.GetRoomsCount());
            Assert.AreEqual(1, _roomManager.GetRoomsCount());
            var roomsList = _roomManager.GetAllRooms();
            _mmRoomManager.UpdateRoomState(roomsList[0].GetRoomId(), 1, RoomState.Open);
            var client1 = new ShamanClientPeerLegacy(new TestMessageDeserializer(), _clientLogger, taskSchedulerFactory, 20, serializer, requestSender);
            var sessionId1 = Guid.NewGuid();
            var success = false;
            client1.GetGames(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM,1, sessionId1, new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 2} },
                new Dictionary<byte, object>(), 
                (rooms) =>
                {
                    Assert.IsNotNull(rooms);
                    Assert.AreEqual(1, rooms.Count);
                    client1.JoinGame(rooms[0].RoomId,                
                    (status, joinInfo) =>
                    {
                        if (joinInfo == null)
                            _clientLogger.Info($"Client status changed {status.Status}, isSuccess = {status.IsSuccess}, error = {status.Error}, joinInfo = null");
                        else
                        {
                            _clientLogger.Info($"Client status changed {status.Status}, isSuccess = {status.IsSuccess}, error = {status.Error}, joinInfo.JoinStatus = {joinInfo.Status}, joinInfo.CurrentPlayers = {joinInfo.CurrentPlayers}, joinInfo.MaxPlayers = {joinInfo.MaxPlayers}");
                            if (joinInfo.Status == JoinStatus.RoomIsReady)
                            {
                                success = true;
                            }
                        }
                        
                        
                    } );
                });
            EmptyTask.Wait(TimeSpan.FromSeconds(2));
            Assert.AreEqual(ClientStatusLegacy.InRoom, client1.GetStatus());
            Assert.AreEqual(1, _mmRoomManager.GetRoomsCount());
            Assert.AreEqual(1, _roomManager.GetRoomsCount());
            
            Assert.AreEqual(true, success);
        }
        
        [Test]
        public void TestCreateGame()
        {
            var client = new ShamanClientPeerLegacy(new TestMessageDeserializer(), _clientLogger, taskSchedulerFactory, 20, serializer, requestSender);
            var sessionId = Guid.NewGuid();
            client.CreateGame(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM,1, sessionId, new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 2} },
                new Dictionary<byte, object>(), 
                (status, joinInfo) =>
                {
                    if (joinInfo == null)
                        _clientLogger.Info($"Client status changed {status.Status}, isSuccess = {status.IsSuccess}, error = {status.Error}, joinInfo = null");
                    else
                    {
                        _clientLogger.Info($"Client status changed {status.Status}, isSuccess = {status.IsSuccess}, error = {status.Error}, joinInfo.JoinStatus = {joinInfo.Status}, joinInfo.CurrentPlayers = {joinInfo.CurrentPlayers}, joinInfo.MaxPlayers = {joinInfo.MaxPlayers}");
                    }
                });
            
            EmptyTask.Wait(TimeSpan.FromSeconds(3));
            Assert.AreEqual(ClientStatusLegacy.InRoom, client.GetStatus());
            Assert.AreEqual(1, _mmRoomManager.GetRoomsCount());
            Assert.AreEqual(1, _roomManager.GetRoomsCount());
        }
        
    }
}