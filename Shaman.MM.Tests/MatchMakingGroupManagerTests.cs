using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages;
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
        private IBotManager _botManager;
        private IMatchMakerServerInfoProvider _serverProvider;
        private IMatchMakingGroupsManager _matchMakingGroupManager;
        
        private Task emptyTask = new Task(() => {});
        private Dictionary<byte, object> _roomProperties = new Dictionary<byte, object>();
        private Dictionary<byte, object> _measures = new Dictionary<byte, object>();
        private Dictionary<byte, object> _playerProperties;
        private List<Guid> _groupIds = new List<Guid>();
        
        [SetUp]
        public void Setup()
        {
            _logger = new ConsoleLogger();
            _taskSchedulerFactory = new TaskSchedulerFactory(_logger);
            _playersManager = new PlayersManager(Mock.Of<IMmMetrics>(), _logger);
            _packetSender = new FakePacketSender();
            _serverProvider = new FakeServerProvider();
            _roomManager = new RoomManager(_serverProvider, _logger, _taskSchedulerFactory);
            _botManager = new BotManager();
            
            _roomProperties.Add(PropertyCode.RoomProperties.MatchMakingTick, 250);
            _roomProperties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded, 3);
            _roomProperties.Add(PropertyCode.RoomProperties.ToAddBots, true);
            _roomProperties.Add(PropertyCode.RoomProperties.ToAddOtherPlayers, true);
            _roomProperties.Add(PropertyCode.RoomProperties.TimeBeforeBotsAdded, 500);
            _roomProperties.Add(PropertyCode.RoomProperties.RoomIsClosingIn, 5000);

            _measures.Add(PropertyCode.PlayerProperties.GameMode, 1);
            
            _playerProperties = new Dictionary<byte, object>
                {{PropertyCode.PlayerProperties.GameMode, 1}};
            
            _matchMakingGroupManager = new MatchMakingGroupManager(_logger, _taskSchedulerFactory, _playersManager,
                _packetSender, Mock.Of<IMmMetrics>(), _serverProvider, _roomManager, _botManager);
            _matchMakingGroupManager.AddMatchMakingGroup(_roomProperties, _measures);
            _groupIds = _matchMakingGroupManager.GetMatchmakingGroupIds(_playerProperties);
            _matchMakingGroupManager.Start(10000);
        }

        [TearDown]
        public void TearDown()
        {
            _roomProperties.Clear();
            _measures.Clear();
            _matchMakingGroupManager.Stop();
        }

        [Test]
        public void OnePlayerMatchMakingTest()
        {
            //one player two bots
            var player = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            _playersManager.Add(player, _groupIds);
            emptyTask.Wait(1500);
            var rooms = _roomManager.GetRooms(_groupIds[0]);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(3, room.Players.Count);
            Assert.AreEqual(2, room.BotsAdded);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(2));
        }
        
        [Test]
        public void TwoPlayers1MatchMakingTest()
        {
            //one player two bots
            var player1 = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            var player2 = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            _playersManager.Add(player1, _groupIds);
            _playersManager.Add(player2, _groupIds);
            emptyTask.Wait(1500);
            var rooms = _roomManager.GetRooms(_groupIds[0]);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(3, room.Players.Count);
            Assert.AreEqual(1, room.BotsAdded);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1));
        }
        
        [Test]
        public void TwoPlayers2MatchMakingTest()
        {
            //one player two bots
            var player1 = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            var player2 = new MatchMakingPlayer(new FakePeer(), _playerProperties);
            _playersManager.Add(player1, _groupIds);
            emptyTask.Wait(1500);
            var rooms = _roomManager.GetRooms(_groupIds[0]);
            Assert.AreEqual(1, rooms.Count());
            var room = rooms.FirstOrDefault();
            Assert.AreEqual(3, room.Players.Count);
            Assert.AreEqual(2, room.BotsAdded);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1));
            
            //join second 
            _playersManager.Add(player2, new List<Guid> {_groupIds[0]});
            emptyTask.Wait(500);
            rooms = _roomManager.GetRooms(_groupIds[0]);
            Assert.AreEqual(1, rooms.Count());
            room = rooms.FirstOrDefault();
            Assert.AreEqual(3, room.Players.Count);
            Assert.AreEqual(1, room.BotsAdded);
            Assert.AreEqual(true, room.IsOpen());
            Assert.AreEqual(true, room.CanJoin(1));
        }
    }
}