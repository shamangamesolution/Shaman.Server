using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.Messages;
using Shaman.Messages.General.Entity;
using Shaman.Messages.MM;
using Shaman.MM.Configuration;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Players;
using Shaman.MM.Providers;
using Shaman.MM.Tests.Fakes;

namespace Shaman.MM.Tests
{
    [TestFixture]
    public class MatchMakingGroupManagerTests
    {
        private IShamanLogger _logger;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private IPlayersManager _playersManager;
        private IPacketSender _packetSender;
        private IRoomManager _roomManager;
        private IMatchMakerServerInfoProvider _serverProvider;
        private IMatchMakingGroupsManager _matchMakingGroupManager;
        private IRoomPropertiesProvider _roomPropertiesProvider;
        
        private Task emptyTask = new Task(() => {});
        private Dictionary<byte, object> _measures = new Dictionary<byte, object>();
        private Dictionary<byte, object> _playerProperties;
        
        [SetUp]
        public void Setup()
        {
            var config = new MmApplicationConfig("", "127.0.0.1", new List<ushort> {0}, "", 120000, GameProject.DefaultGame,"", 7002, isAuthOn:false);
            _logger = new ConsoleLogger();
            _roomPropertiesProvider = new FakeRoomPropertiesProvider(3, 500, 250);
            
            _taskSchedulerFactory = new TaskSchedulerFactory(_logger);
            _playersManager = new PlayersManager(Mock.Of<IMmMetrics>(), _logger);
            _packetSender = Mock.Of<IPacketSender>();
            _serverProvider = new FakeServerProvider();
            _roomManager = new RoomManager(_serverProvider, _logger, _taskSchedulerFactory);

            _measures.Add(PropertyCode.PlayerProperties.GameMode, 1);
            
            _playerProperties = new Dictionary<byte, object>
                {{PropertyCode.PlayerProperties.GameMode, 1}};
            
            _matchMakingGroupManager = new MatchMakingGroupManager(_logger, _taskSchedulerFactory, _playersManager,
                _packetSender, Mock.Of<IMmMetrics>(), _roomManager, _roomPropertiesProvider, config);
            _matchMakingGroupManager.AddMatchMakingGroup(_measures);
            _matchMakingGroupManager.Start(10000);
        }

        [TearDown]
        public void TearDown()
        {
            _measures.Clear();
            _matchMakingGroupManager.Stop();
        }

        [Test]
        public void OnePlayerMatchMakingTest()
        {
            //one player two bots
            var player = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            _matchMakingGroupManager.AddPlayerToMatchMaking(player);
            
            emptyTask.Wait(1500);
            var rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetAllRooms();
            _roomManager.UpdateRoomState(rooms.First().Id, 1, RoomState.Open);
            rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(1, room.CurrentPlayersCount);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(2));
        }
        
        [Test]
        public void TwoPlayers1MatchMakingTest()
        {
            //one player two bots
            var player1 = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            var player2 = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            _matchMakingGroupManager.AddPlayerToMatchMaking(player1);
            _matchMakingGroupManager.AddPlayerToMatchMaking(player2);
            emptyTask.Wait(1500);
            var rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetAllRooms();
            _roomManager.UpdateRoomState(rooms.First().Id, 2, RoomState.Open);
            rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(2, room.CurrentPlayersCount);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1));
        }
        
        [Test]
        public void TwoPlayers2MatchMakingTest()
        {
            //one player two bots
            var player1 = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            var player2 = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            _matchMakingGroupManager.AddPlayerToMatchMaking(player1);
            emptyTask.Wait(1500);
            var rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetAllRooms();
            _roomManager.UpdateRoomState(rooms.First().Id, 1, RoomState.Open);
            rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(1, room.CurrentPlayersCount);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1));
            
            //join second 
            _matchMakingGroupManager.AddPlayerToMatchMaking(player2);
            emptyTask.Wait(500);
            rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(1, rooms.Count());
            room = rooms.FirstOrDefault();
            Assert.AreEqual(2, room.CurrentPlayersCount);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1));
        }
    }
}