using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Http;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle.Stats;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.Contract.Routing.MM;
using Shaman.Messages.MM;
using Shaman.MM.Managers;
using Shaman.MM.Players;
using Shaman.MM.Providers;
using Shaman.MM.Tests.Fakes;
using Shaman.TestTools.Events;

namespace Shaman.MM.Tests
{
    [TestFixture]
    public class MatchMakingGroupManagerTests
    {
        private IShamanLogger _logger;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private IPlayersManager _playersManager;
        private IShamanMessageSender _packetSender;
        private IRoomManager _roomManager;
        private IMatchMakerServerInfoProvider _serverProvider;
        private IMatchMakingGroupsManager _matchMakingGroupManager;
        private IRoomPropertiesProvider _roomPropertiesProvider;
        private IRequestSender _requestSender;
        private IRoomApiProvider _roomApiProvider;

        private Task emptyTask = new Task(() => {});
        private Dictionary<byte, object> _measures = new Dictionary<byte, object>();
        private Dictionary<byte, object> _playerProperties;
        
        [SetUp]
        public void Setup()
        {
            // var config = new MmApplicationConfig("", "127.0.0.1", new List<ushort> {0}, "", 120000, GameProject.DefaultGame,"", 7002, isAuthOn:false);
            var config = new ApplicationConfig
            {
                PublicDomainNameOrAddress = "127.0.0.1",
                ListenPorts = "0",
                IsAuthOn = false,
                BindToPortHttp = 7002
            };
            _logger = new ConsoleLogger();
            _roomPropertiesProvider = new FakeRoomPropertiesProvider(3, 500, 250, 3);
            
            _taskSchedulerFactory = new TaskSchedulerFactory(_logger);
            _playersManager = new PlayersManager(Mock.Of<IMmMetrics>(), _logger);
            _packetSender = Mock.Of<IShamanMessageSender>();
            _serverProvider = new FakeServerProvider();
            _requestSender = new FakeSender();
            _roomApiProvider = new DefaultRoomApiProvider(_requestSender,_logger);
            _roomManager = new RoomManager(_serverProvider, _logger, _taskSchedulerFactory, _roomApiProvider);

            _measures.Add(FakePropertyCodes.PlayerProperties.GameMode, 1);
            
            _playerProperties = new Dictionary<byte, object>
                {{FakePropertyCodes.PlayerProperties.GameMode, 1}};
            
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
            _roomManager.UpdateRoomState(rooms.First().Id, 1, RoomState.Open, 1);
            rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(1, room.CurrentWeight);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(2, 1));
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
            _roomManager.UpdateRoomState(rooms.First().Id, 2, RoomState.Open, 1);
            rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(2, room.CurrentWeight);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1, 1));
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
            _roomManager.UpdateRoomState(rooms.First().Id, 1, RoomState.Open, 1);
            rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(1, room.CurrentWeight);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1,1));
            
            //join second 
            _roomManager.UpdateRoomState(room.Id, 1, RoomState.Open, 1);
            _matchMakingGroupManager.AddPlayerToMatchMaking(player2);
            emptyTask.Wait(500);
            //open again - it was closed after adding a player
            _roomManager.UpdateRoomState(room.Id, 2, RoomState.Open, 1);
            rooms = _matchMakingGroupManager.GetRooms(_playerProperties);
            Assert.AreEqual(1, rooms.Count());
            room = rooms.FirstOrDefault();
            Assert.AreEqual(2, room.CurrentWeight);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1,1));
        }
    }
}