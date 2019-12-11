using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shaman.Client;
using Shaman.Client.Peers;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.Logging;
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
using Shaman.MM.Players;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.Tests.Providers;
using Shaman.TestTools.ClientPeers;
using ClientStatus = Shaman.Client.Peers.ClientStatus;
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

        private IPlayerCollection playerCollection = null; 
        private IMatchMaker matchMaker;
        private List<MatchMakingGroup> matchMakingGroups = new List<MatchMakingGroup>();
        private IRequestSender requestSender = null;
        private List<ShamanClientPeer> _clients = new List<ShamanClientPeer>();
        private IBackendProvider _backendProvider;
        private IRoomPropertiesContainer _roomPropertiesContainer;
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
            var config = new MmApplicationConfig("", "127.0.0.1", new List<ushort> {SERVER_PORT_MM}, "", 120000, 120000, GameProject.DefaultGame,"", isAuthOn:false);
            playerCollection = new PlayerCollection(_serverLogger, Mock.Of<IMmMetrics>());
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            //fake sender to direct calls of application functions
            requestSender = new FakeSenderWithGameApplication(new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte)GameMode.SinglePlayer}}, CreateRoomDelegate, UpdateRoomDelegate);
            _backendProvider = new BackendProvider(taskSchedulerFactory, config, requestSender, _serverLogger);
            var gameConfig = new GameApplicationConfig("", "", "127.0.0.1", new List<ushort> {SERVER_PORT_GAME}, "", "", 7000,
                isAuthOn: false);
            _mmPacketSender = new PacketBatchSender(taskSchedulerFactory, config, serializerFactory);
            _gamePacketSender = new PacketBatchSender(taskSchedulerFactory, gameConfig, serializerFactory);

            var createdRoomManager = new CreatedRoomManager(taskSchedulerFactory, _serverLogger);
            _statsProvider = new MM.Providers.StatisticsProvider(playerCollection);
            _serverProvider = new FakeMatchMakerServerInfoProvider(requestSender, "127.0.0.1", $"{SERVER_PORT_GAME}");

            _playerManager = new PlayersManager( Mock.Of<IMmMetrics>(), _serverLogger);
            _mmRoomManager =
                new MM.Managers.RoomManager(_serverProvider, _serverLogger, taskSchedulerFactory.GetTaskScheduler());
            _botManager = new BotManager();
            _mmGroupManager = new MatchMakingGroupManager(_serverLogger, taskSchedulerFactory, _playerManager, _mmPacketSender,  Mock.Of<IMmMetrics>(), _serverProvider, _mmRoomManager, _botManager);
            matchMaker = new MatchMaker(playerCollection, _serverLogger,  _mmPacketSender, Mock.Of<IMmMetrics>(), createdRoomManager, _playerManager, _mmGroupManager);
            _roomProperties = new Dictionary<byte, object>();
            _roomProperties.Add(PropertyCode.RoomProperties.MatchMakingTick, MM_TICK);
            _roomProperties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded, TOTAL_PLAYERS_NEEDED_1);
            _roomProperties.Add(PropertyCode.RoomProperties.ToAddBots, true);
            _roomProperties.Add(PropertyCode.RoomProperties.ToAddOtherPlayers, true);
            _roomProperties.Add(PropertyCode.RoomProperties.TimeBeforeBotsAdded, 5000);
            _roomProperties.Add(PropertyCode.RoomProperties.RoomIsClosingIn, 120000);
            _measures = new Dictionary<byte, object>();
            _measures.Add(PropertyCode.PlayerProperties.Level, 1);
            //matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_1, MM_TICK, true, true, 5000, 120000, new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte)GameMode.SinglePlayer}}, new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 1}});
            matchMaker.AddMatchMakingGroup(_roomProperties, _measures);
            
            //setup mm server
            _mmApplication = new MmApplication(_serverLogger, config, serializerFactory, socketFactory, playerCollection, matchMaker,requestSender, taskSchedulerFactory, _backendProvider, _mmPacketSender, createdRoomManager, _serverProvider, _mmRoomManager);
            matchMaker.AddRequiredProperty(PropertyCode.PlayerProperties.Level);
            
            _mmApplication.Start();
            
            //setup game server
            _roomPropertiesContainer = new RoomPropertiesContainer(_serverLogger);
            _gameModeControllerFactory = new FakeGameModeControllerFactory();

            _roomManager = new RoomManager(_serverLogger, serializerFactory, gameConfig, taskSchedulerFactory,
                _gameModeControllerFactory, _mmPacketSender, Mock.Of<IGameMetrics>());

            _gameApplication = new GameApplication(_serverLogger, gameConfig, serializerFactory, socketFactory, taskSchedulerFactory, requestSender, _backendProvider, _roomManager, _gamePacketSender);
            _gameApplication.Start();
            
            _clientLogger.SetLogLevel(LogLevel.Error);
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
                return MessageFactory.DeserializeMessageForTest(operationCode, serializer, message, 0, message.Length);
            }
        }
        
        [Test]
        public void FullCycleTest()
        {
            for (int i = 0; i < CLIENTS_NUMBER_1; i++)
            {
                var client = new ShamanClientPeer(new TestMessageDeserializer(), _clientLogger, taskSchedulerFactory, 20, serializerFactory, requestSender);
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
                if (client.GetStatus() != ClientStatus.InRoom)
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
                if (client.GetStatus() != ClientStatus.InRoom)
                    success = false;
            
            Assert.AreEqual(true, success);

            foreach (var item in _eventsCount)
            {
                Assert.AreEqual(item.Value, EVENTS_SENT * (TOTAL_PLAYERS_NEEDED_1 - 1));
            }
        }
    }
}