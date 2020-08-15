using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Messages;
using Shaman.Messages.MM;
using Shaman.MM.Managers;
using Shaman.MM.Providers;
using Shaman.MM.Tests.Fakes;

namespace Shaman.MM.Tests
{
    [TestFixture]
    public class RoomManagerTests
    {
        private IMatchMakerServerInfoProvider _serverProvider;
        private IRoomManager _roomManager;
        private IShamanLogger _logger;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private Task emptyTask = new Task(() => {});
        private Guid _group1Id = Guid.NewGuid();
        
        [SetUp]
        public void Setup()
        {
            _logger = new ConsoleLogger();
            _serverProvider = new FakeServerProvider();
            _taskSchedulerFactory = new TaskSchedulerFactory(_logger);
            _roomManager = new RoomManager(_serverProvider, _logger, _taskSchedulerFactory);
            _roomManager.Start(5);
        }

        [TearDown]
        public void TearDown()
        {
            _roomManager.Stop();
        }

        [Test]
        public async Task CreateRoomTest()
        {
            var players = new Dictionary<Guid, Dictionary<byte, object>>
            {
                {Guid.NewGuid(), new Dictionary<byte, object>()}
            };
            var properties = new Dictionary<byte, object>();
           
            var success = true;
            Exception ex = null;
            JoinRoomResult result = null;
            try
            {
                result = await _roomManager.CreateRoom(_group1Id, players, properties);
            }
            catch (Exception e)
            {
                success = false;
                ex = e;
            }
            Assert.IsNull(result);
            Assert.AreEqual(false, success);
            Assert.AreEqual("MatchMakingGroup ctr error: there is no TotalPlayersNeeded property", ex.Message);
            Assert.AreEqual(0, _roomManager.GetRoomsCount());

            properties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded, 3);
            
            success = true;
            ex = null;
            result = null;
            try
            {
                result = await _roomManager.CreateRoom(_group1Id, players, properties);
            }
            catch (Exception e)
            {
                success = false;
                ex = e;
            }
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Result, RoomOperationResult.OK);
            Assert.AreEqual(result.Address, "0.0.0.0");
            Assert.AreEqual(result.Port, 7777);
            Assert.AreEqual(true, success);
            Assert.AreEqual(null, ex);
            Assert.AreEqual(1, _roomManager.GetRoomsCount());
        }

        [Test]
        public async Task CreateRoomNoServersTest()
        {
            _serverProvider = new FakeServerProvider(false, true);
            _roomManager = new RoomManager(_serverProvider, _logger, _taskSchedulerFactory);

            var players = new Dictionary<Guid, Dictionary<byte, object>>();
            var properties = new Dictionary<byte, object>();
            var success = true;
            Exception ex = null;
            JoinRoomResult result = null;
            
            properties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded, 3);
            try
            {
                result = await _roomManager.CreateRoom(_group1Id, players, properties);
            }
            catch (Exception e)
            {
                success = false;
                ex = e;
            }
            
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Result, RoomOperationResult.ServerNotFound);
            Assert.AreEqual(true, success);
            Assert.AreEqual(null, ex);
            Assert.AreEqual(0, _roomManager.GetRoomsCount());
        }
        
        [Test]
        public async Task CreateRoomRoomEmptyTest()
        {
            _serverProvider = new FakeServerProvider(returnEmptyGuid: true);
            _roomManager = new RoomManager(_serverProvider, _logger, _taskSchedulerFactory);

            var players = new Dictionary<Guid, Dictionary<byte, object>>();
            var properties = new Dictionary<byte, object>();
            var success = true;
            Exception ex = null;
            JoinRoomResult result = null;
            
            properties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded, 3);
            try
            {
                result = await _roomManager.CreateRoom(_group1Id, players, properties);
            }
            catch (Exception e)
            {
                success = false;
                ex = e;
            }
            
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Result, RoomOperationResult.CreateRoomError);
            Assert.AreEqual(true, success);
            Assert.AreEqual(null, ex);
            Assert.AreEqual(0, _roomManager.GetRoomsCount());
        }

        [Test]
        public async Task GetRoomTest()
        {
            //create room
            var players = new Dictionary<Guid, Dictionary<byte, object>>
            {
                {Guid.NewGuid(), new Dictionary<byte, object>()}
            };

            var properties = new Dictionary<byte, object>();
            
            properties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded, 3);
            
            var result = await _roomManager.CreateRoom(_group1Id, players, properties);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Result, RoomOperationResult.OK);
            Assert.AreEqual(result.Address, "0.0.0.0");
            Assert.AreEqual(result.Port, 7777);
            
            emptyTask.Wait(1000);
            //success
            var room = _roomManager.GetRoom(_group1Id, 1);
            Assert.IsNull(room);
            _roomManager.UpdateRoomState(result.RoomId, 1, RoomState.Open);
            room = _roomManager.GetRoom(_group1Id, 1);
            Assert.IsNotNull(room);
            //success
            room = _roomManager.GetRoom(_group1Id, 2);
            Assert.IsNotNull(room);
            //no such room
            room = _roomManager.GetRoom(_group1Id, 3);
            Assert.IsNull(room);

            var rooms = _roomManager.GetRooms(_group1Id, true);
            Assert.AreEqual(1, rooms.Count());
            
            emptyTask.Wait(3100);
            
            //room is closed by this time
            _roomManager.UpdateRoomState(result.RoomId, 1, RoomState.Closed);
            //no such room
            room = _roomManager.GetRoom(_group1Id, 1);
            Assert.IsNull(room);
            //no such room
            room = _roomManager.GetRoom(_group1Id, 2);
            Assert.IsNull(room);
            //no such room
            room = _roomManager.GetRoom(_group1Id, 3);
            Assert.IsNull(room);
            room = _roomManager.GetRoom(Guid.NewGuid(), 3);
            Assert.IsNull(room);
            rooms = _roomManager.GetRooms(_group1Id, true);
            Assert.AreEqual(0, rooms.Count());
            rooms = _roomManager.GetRooms(_group1Id, false);
            Assert.AreEqual(1, rooms.Count());
            rooms = _roomManager.GetRooms(Guid.NewGuid(), false);
            Assert.AreEqual(0, rooms.Count());
            emptyTask.Wait(3000);
            //room should be deleted
            rooms = _roomManager.GetRooms(_group1Id, false);
            Assert.AreEqual(0, rooms.Count()); 
        }

        [Test]
        public async Task JoinNoServersTest()
        {
            var players = new Dictionary<Guid, Dictionary<byte, object>>
            {
                {Guid.NewGuid(), new Dictionary<byte, object>()}
            };
            var properties = new Dictionary<byte, object>();
            
            properties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded, 2);
            
            var result = await _roomManager.CreateRoom(_group1Id, players, properties);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Result, RoomOperationResult.OK);
            Assert.AreEqual(result.Address, "0.0.0.0");
            Assert.AreEqual(result.Port, 7777);
            
            var room = _roomManager.GetRoom(_group1Id, 1);
            Assert.IsNull(room);
            _roomManager.UpdateRoomState(result.RoomId, 1, RoomState.Open);
            room = _roomManager.GetRoom(_group1Id, 1);
            Assert.IsNotNull(room);

            players = new Dictionary<Guid, Dictionary<byte, object>>
            {
                {Guid.NewGuid(), new Dictionary<byte, object>()}
            };
            result = await _roomManager.JoinRoom(result.RoomId, players);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Result, RoomOperationResult.OK);
            Assert.AreEqual(result.Address, "0.0.0.0");
            Assert.AreEqual(result.Port, 7777);
            
            //no empty room anymore
            room = _roomManager.GetRoom(_group1Id, 1);
            Assert.IsNull(room);
            
            //join to closed room
            players = new Dictionary<Guid, Dictionary<byte, object>>
            {
                {Guid.NewGuid(), new Dictionary<byte, object>()}
            };
            result = await _roomManager.JoinRoom(result.RoomId, players);
            Assert.IsNotNull(result);
            Assert.AreEqual(RoomOperationResult.JoinRoomError, result.Result);
        }
        
        [Test]
        public async Task JoinTest()
        {
            var players = new Dictionary<Guid, Dictionary<byte, object>>
            {
                {Guid.NewGuid(), new Dictionary<byte, object>()}
            };

            var properties = new Dictionary<byte, object>();
            
            properties.Add(PropertyCode.RoomProperties.TotalPlayersNeeded, 2);
            
            var result = await _roomManager.CreateRoom(_group1Id, players, properties);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Result, RoomOperationResult.OK);
            Assert.AreEqual(result.Address, "0.0.0.0");
            Assert.AreEqual(result.Port, 7777);
            
            var room = _roomManager.GetRoom(_group1Id, 1);
            Assert.IsNull(room);
            _roomManager.UpdateRoomState(result.RoomId, 1, RoomState.Open);
            room = _roomManager.GetRoom(_group1Id, 1);
            Assert.IsNotNull(room);

            players = new Dictionary<Guid, Dictionary<byte, object>>
            {
                {Guid.NewGuid(), new Dictionary<byte, object>()}
            };
            result = await _roomManager.JoinRoom(result.RoomId, players);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Result, RoomOperationResult.OK);
            Assert.AreEqual(result.Address, "0.0.0.0");
            Assert.AreEqual(result.Port, 7777);
            
            //no empty room anymore
            room = _roomManager.GetRoom(_group1Id, 1);
            Assert.IsNull(room);
            
            //join to closed room
            players = new Dictionary<Guid, Dictionary<byte, object>>
            {
                {Guid.NewGuid(), new Dictionary<byte, object>()}
            };
            result = await _roomManager.JoinRoom(result.RoomId, players);
            Assert.IsNotNull(result);
            Assert.AreEqual(RoomOperationResult.JoinRoomError, result.Result);
        }
    }
}