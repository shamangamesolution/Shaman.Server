using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.MM.Players;
using Shaman.MM.Tests.Fakes;

namespace Shaman.MM.Tests
{
    [TestFixture]
    public class PlayerManagerTests
    {
        private IPlayersManager _playersManager;
        private IShamanLogger _logger;
        private List<Guid> groupList1 = new List<Guid>() {Guid.NewGuid()};
        private List<Guid> groupList2 = new List<Guid>() {Guid.NewGuid()};
        private Task emptyTask = new Task(() => {});
        
        [SetUp]
        public void Setup()
        {
            _logger = new ConsoleLogger();
            _playersManager = new PlayersManager(Mock.Of<IMmMetrics>(), _logger);
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void AddRemoveTest()
        {
            
            var player = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object>());
            var playerId = player.Id;
            _playersManager.Add(player, groupList1);
            var newPlayer = _playersManager.GetPlayer(playerId);
            Assert.AreEqual(player, newPlayer);
            Assert.AreEqual(1, _playersManager.Count());
            _playersManager.Remove(playerId);
            Assert.AreEqual(0, _playersManager.Count());
            Assert.IsNull(_playersManager.GetPlayer(Guid.NewGuid()));
        }


        [Test]
        public void GetOldestPlayerTest()
        {
            Assert.IsNull(_playersManager.GetOldestPlayer());
            var player1 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object>());
            var player2 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object>());
            var player3 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object>());
            _playersManager.Add(player2, groupList1);
            emptyTask.Wait(100);
            _playersManager.Add(player1, groupList1);
            _playersManager.Add(player3, groupList2);
            var oldestPlayer = _playersManager.GetOldestPlayer();
            Assert.AreEqual(player2, oldestPlayer);
            _playersManager.Remove(Guid.NewGuid());
            _playersManager.Remove(player1.Id);
            _playersManager.Remove(player2.Id);
            oldestPlayer = _playersManager.GetOldestPlayer();
            Assert.AreEqual(player3, oldestPlayer);
            Assert.AreEqual(1, _playersManager.Count());
        }

        [Test]
        public void GetPlayerByGroup()
        {
            var player1 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object>());
            var player2 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object>());
            var player3 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object>());
            _playersManager.Add(player2, groupList1);
            _playersManager.Add(player1, groupList1);
            _playersManager.Add(player3, groupList2);
            Assert.AreEqual(0, _playersManager.GetPlayers(groupList1[0], 0).Count());
            Assert.AreEqual(0, _playersManager.GetPlayers(Guid.NewGuid(), 1).Count());
            Assert.AreEqual(1, _playersManager.GetPlayers(groupList1[0], 1).Count());
            Assert.AreEqual(2, _playersManager.GetPlayers(groupList1[0], 2).Count());
            Assert.AreEqual(1, _playersManager.GetPlayers(groupList2[0], 1).Count());
            Assert.AreEqual(3, _playersManager.Count());
            _playersManager.Clear();
            Assert.AreEqual(0, _playersManager.Count());
        }

        [Test]
        public void SetOnMatchMakingTest()
        {
            var player1 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object>());
            var player2 = new MatchMakingPlayer(new FakePeer(), new Dictionary<byte, object>());
            _playersManager.Add(player2, groupList1);
            _playersManager.Add(player1, groupList1);
            Assert.AreEqual(2, _playersManager.GetPlayers(groupList1[0], 2).Count());
            _playersManager.SetOnMatchmaking(player2.Id, true);
            Assert.AreEqual(1, _playersManager.GetPlayers(groupList1[0], 2).Count());
            _playersManager.SetOnMatchmaking(player2.Id, false);
            Assert.AreEqual(2, _playersManager.GetPlayers(groupList1[0], 2).Count());

            
        }
    }
}