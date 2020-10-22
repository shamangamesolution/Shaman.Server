using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shaman.Client;
using Shaman.Client.Peers;
using Shaman.Common.Server.Providers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.Contract.Routing.MM;
using Shaman.Game;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms.RoomProperties;
using Shaman.MM;
using Shaman.MM.MatchMaking;
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;
using Shaman.Tests.Helpers;
using Shaman.Tests.Providers;
using Shaman.TestTools.ClientPeers;
using Shaman.TestTools.Events;
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
        private IRoomPropertiesContainer _roomPropertiesContainer;
        private IRoomManager _roomManager;
        private IRoomControllerFactory _roomControllerFactory;
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
            _gameApplication = InstanceHelper.GetGame(SERVER_PORT_GAME);
            _mmApplication = InstanceHelper.GetMm(SERVER_PORT_MM, SERVER_PORT_GAME, _gameApplication);

            _mmApplication.Start();
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
                client.JoinGame(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM,1, sessionId, new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 1} },
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
                client.RegisterOperationHandler(TestEventCodes.TestEventCode, message =>
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
            client.JoinGame(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM,1, sessionId, new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 2} },
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
            _mmRoomManager.UpdateRoomState(roomsList[0].GetRoomId(), 1, RoomState.Open, 1);
            var client1 = new ShamanClientPeerLegacy(new TestMessageDeserializer(), _clientLogger, taskSchedulerFactory, 20, serializer, requestSender);
            var sessionId1 = Guid.NewGuid();
            var success = false;
            client1.GetGames(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM,1, sessionId1, new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 2} },
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
            client.CreateGame(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM,1, sessionId, new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 2} },
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