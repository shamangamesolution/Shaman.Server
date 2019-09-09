using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework;
using Shaman.Client.Peers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
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
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Messages.MM;
using ClientStatus = Shaman.Client.Peers.ClientStatus;

namespace Shaman.Tests
{
    public class SimpleClientTests : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort SERVER_PORT_GAME = 23451;
        private const ushort SERVER_PORT_MM = 23452;
        private const ushort WAIT_TIMEOUT = 100;
        private const ushort MM_TICK = 1000;
        private const ushort CLIENTS_NUMBER_1 = 24;
        private const ushort CLIENTS_NUMBER_2 = 1500;
        
        private const ushort TOTAL_PLAYERS_NEEDED_1 = 6;
        private const ushort EVENTS_SENT = 100;
        
        private GameApplication _gameApplication;
        private MmApplication _mmApplication;

        private IRegisteredServerCollection serverCollection = null; 
        private IPlayerCollection playerCollection = null; 
        private IMatchMaker matchMaker;
        private List<MatchMakingGroup> matchMakingGroups = new List<MatchMakingGroup>();
        private IRequestSender requestSender = null;
        private List<ShamanClientPeer> _clients = new List<ShamanClientPeer>();
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
            var config = new MmApplicationConfig("127.0.0.1", new ushort[] {SERVER_PORT_MM}, "", 120000, 120000, GameProject.DefaultGame,"","", isAuthOn:false);
            playerCollection = new PlayerCollection(_serverLogger,serializerFactory);
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            serverCollection = new RegisteredServersCollection(_serverLogger, config, taskSchedulerFactory);
            //fake sender to direct calls of application functions
            requestSender = new FakeSenderWithGameApplication(new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte)GameMode.DefaultGameMode}}, CreateRoomDelegate, ActualizeServerDelegate);
            _backendProvider = new BackendProvider(taskSchedulerFactory, config, requestSender, _serverLogger);
            var gameConfig = new GameApplicationConfig("127.0.0.1", new ushort[] {SERVER_PORT_GAME}, "", "", 7000,
                isAuthOn: false);
            _packetSender = new PacketBatchSender(taskSchedulerFactory, gameConfig, serializerFactory);
            
            matchMaker = new MatchMaker(serverCollection, playerCollection, _serverLogger, taskSchedulerFactory, serializerFactory, _packetSender);
            matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_1, MM_TICK, true, true, 5000, new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte)GameMode.DefaultGameMode}}, new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 1}});

            
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
            
            //setup game server
            _roomPropertiesContainer = new RoomPropertiesContainer(_serverLogger);
            _gameModeControllerFactory = new FakeGameModeControllerFactory();

            _roomManager = new RoomManager(_serverLogger, serializerFactory, gameConfig, taskSchedulerFactory, requestSender, _backendProvider, _gameModeControllerFactory, _roomPropertiesContainer, _storageContainer, _packetSender);

            _gameApplication = new GameApplication(_serverLogger, gameConfig, serializerFactory, socketFactory, taskSchedulerFactory, requestSender, _backendProvider, _storageContainer, _roomManager, _packetSender);
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
        [Test]
        public void FullCycleTest()
        {
            for (int i = 0; i < CLIENTS_NUMBER_1; i++)
            {
                var client = new ShamanClientPeer(_clientLogger, taskSchedulerFactory, 20, serializerFactory, requestSender);
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