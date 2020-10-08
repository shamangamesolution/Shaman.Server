using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Http;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.MM;
using Shaman.Messages.MM;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.MM.Metrics;
using Shaman.MM.Players;
using Shaman.MM.Providers;
using Shaman.MM.Tests.Fakes;
using Shaman.TestTools.Events;

namespace Shaman.MM.Tests
{
    [TestFixture]
    public class MmWeightTests
    {
        private IShamanLogger _logger;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private IPlayersManager _playersManager;
        private IShamanMessageSender _packetSender;
        private IRoomManager _roomManager;
        private IMatchMakerServerInfoProvider _serverProvider;
        private IRoomApiProvider _roomApiProvider;
        private IRequestSender _requestSender;
        
        private Task emptyTask = new Task(() => {});
        private Dictionary<byte, object> _roomProperties = new Dictionary<byte, object>();
        private Dictionary<byte, object> _measures = new Dictionary<byte, object>();
        
        private MatchMakingGroup _group;
        
        [SetUp]
        public void Setup()
        {
            _logger = new ConsoleLogger();
            _taskSchedulerFactory = new TaskSchedulerFactory(_logger);
            _playersManager = new PlayersManager(Mock.Of<IMmMetrics>(), _logger);
            _packetSender = Mock.Of<IShamanMessageSender>();
            _serverProvider = new FakeServerProvider();
            _requestSender = new FakeSender();
            _roomApiProvider = new DefaultRoomApiProvider(_requestSender,_logger);
            _roomManager = new RoomManager(_serverProvider, _logger, _taskSchedulerFactory, _roomApiProvider);
            
            _measures.Add(FakePropertyCodes.PlayerProperties.GameMode, 1);
            _roomProperties.Add(FakePropertyCodes.RoomProperties.MatchMakingTick, 250);
            _roomProperties.Add(FakePropertyCodes.RoomProperties.TotalPlayersNeeded, 12);
            _roomProperties.Add(FakePropertyCodes.RoomProperties.MaximumMmTime, 500);
            _roomProperties.Add(FakePropertyCodes.RoomProperties.MaximumMatchMakingWeight, 6);

            
            _group = new MatchMakingGroup(_roomProperties, _logger, _taskSchedulerFactory, _playersManager,
                _packetSender, Mock.Of<IMmMetrics>(), _roomManager);
            _roomManager.Start(10000);
            _group.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _roomProperties.Clear();
            _measures.Clear();
            _group.Stop();
            _roomManager.Stop();
        }

        [Test]
        public void WeightJoinTest()
        {
            //first player
            var player1 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}}, 1);
            //second player with weight > 1 (team probably)
            var player2 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}}, 6);
            _playersManager.Add(player1, new List<Guid> {_group.Id});
            _playersManager.Add(player2, new List<Guid> {_group.Id});
            emptyTask.Wait(1500);
            var rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetRooms(_group.Id, false);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.First();
            Assert.AreEqual(RoomState.Closed, room.State);
            Assert.AreEqual(0, room.MaxWeightToJoin);
            Assert.AreEqual(2, room.CurrentWeight);
            Assert.AreEqual(12, room.TotalWeightNeeded);
            //update state
            _roomManager.UpdateRoomState(rooms.First().Id, 2, RoomState.Open, 5);
            rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(1, rooms.Count());
            room = rooms.FirstOrDefault();
            Assert.AreEqual(2, room.CurrentWeight);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(5,5));
            //third player
            var player3 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}}, 5);
            _playersManager.Add(player3, new List<Guid> {_group.Id});
            emptyTask.Wait(1500);
            rooms = _roomManager.GetRooms(_group.Id, false);
            Assert.AreEqual(1, rooms.Count());
            room = rooms.FirstOrDefault();
            Assert.AreEqual(3, room.CurrentWeight);
            Assert.AreEqual(false, room.IsOpen());
            Assert.AreEqual(false, room.CanJoin(1,1));
            Assert.AreEqual(0, room.MaxWeightToJoin);
            Assert.AreEqual(3, room.CurrentWeight);
            Assert.AreEqual(12, room.TotalWeightNeeded);
        }

        [Test]
        public void TwoTeamsMmJoin()
        {
            //first player
            var player1 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}}, 6);
            //second player 
            var player2 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}}, 6);
            _playersManager.Add(player1, new List<Guid> {_group.Id});
            _playersManager.Add(player2, new List<Guid> {_group.Id});
            emptyTask.Wait(1500);
            var rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetRooms(_group.Id, false);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.First();
            Assert.AreEqual(RoomState.Closed, room.State);
            Assert.AreEqual(0, room.MaxWeightToJoin);
            Assert.AreEqual(2, room.CurrentWeight);
            Assert.AreEqual(12, room.TotalWeightNeeded);
        }
        
        [Test]
        public void ThreeTeamsTwoRoomsJoin()
        {
            //first player
            var player1 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}}, 6);
            //second player 
            var player2 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}}, 6);
            //third player 
            var player3 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}}, 6);
            
            _playersManager.Add(player1, new List<Guid> {_group.Id});
            _playersManager.Add(player2, new List<Guid> {_group.Id});
            _playersManager.Add(player3, new List<Guid> {_group.Id});
            emptyTask.Wait(1500);
            var rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetRooms(_group.Id, false);
            Assert.AreEqual(2, rooms.Count());
        }
    }
}