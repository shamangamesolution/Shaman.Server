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
using Shaman.Contract.Bundle.Stats;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.MM;
using Shaman.Messages.MM;
using Shaman.MM.Managers;
using Shaman.MM.MatchMaking;
using Shaman.MM.Players;
using Shaman.MM.Providers;
using Shaman.MM.Tests.Fakes;
using Shaman.TestTools.Events;

namespace Shaman.MM.Tests
{
    [TestFixture]
    public class MatchMakerGroupTest
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
            _roomProperties.Add(FakePropertyCodes.RoomProperties.TotalPlayersNeeded, 3);
            _roomProperties.Add(FakePropertyCodes.RoomProperties.MaximumMmTime, 500);
            _roomProperties.Add(FakePropertyCodes.RoomProperties.MaximumMatchMakingWeight, 1);

            
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
        public void OnePlayerMatchMakingTest()
        {
            //one player two bots
            var player = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}});
            _playersManager.Add(player, new List<Guid> {_group.Id});
            emptyTask.Wait(1500);
            var rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetRooms(_group.Id, false);
            Assert.AreEqual(1, rooms.Count());
            _roomManager.UpdateRoomState(rooms.First().Id, 1, RoomState.Open, 1);
            rooms = _roomManager.GetRooms(_group.Id);
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
            var player1 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}});
            var player2 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}});
            _playersManager.Add(player1, new List<Guid> {_group.Id});
            _playersManager.Add(player2, new List<Guid> {_group.Id});
            emptyTask.Wait(1500);
            var rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetRooms(_group.Id, false);
            Assert.AreEqual(1, rooms.Count());
            _roomManager.UpdateRoomState(rooms.First().Id, 2, RoomState.Open, 1);
            rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(2, room.CurrentWeight);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1,1));
        }
        
        [Test]
        public void TwoPlayers2MatchMakingTest()
        {
            //one player two bots
            var player1 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}});
            var player2 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.GameMode, 1}});
            _playersManager.Add(player1, new List<Guid> {_group.Id});
            emptyTask.Wait(1500);
            var rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetRooms(_group.Id, false);
            Assert.AreEqual(1, rooms.Count());
            _roomManager.UpdateRoomState(rooms.First().Id, 1, RoomState.Open, 1);
            rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(1, room.CurrentWeight);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1,1));
            
            //join second 
            _playersManager.Add(player2, new List<Guid> {_group.Id});
            emptyTask.Wait(500);
            rooms = _roomManager.GetRooms(_group.Id);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetRooms(_group.Id, false);
            room = rooms.FirstOrDefault();
            Assert.AreEqual(2, room.CurrentWeight);
            Assert.AreEqual(false, room.IsOpen());
            _roomManager.UpdateRoomState(room.Id, 1, RoomState.Open, 1);
            Assert.AreEqual(true, room.CanJoin(1,1));
        }
        
        
    }
}