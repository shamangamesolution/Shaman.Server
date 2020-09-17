using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Http;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.MM;
using Shaman.Contract.Routing.MM;
using Shaman.Game;
using Shaman.MM;
using Shaman.MM.MatchMaking;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.Entity;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.Tests.Helpers;
using Shaman.Tests.Providers;
using Shaman.TestTools.ClientPeers;
using Shaman.TestTools.Events;

namespace Shaman.Tests
{    
    [TestFixture]
    [NonParallelizable]
    public class MmTests : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort MM_SERVER_PORT = 23450;
        private const ushort SERVER_PORT = 23451;
        private const ushort WAIT_TIMEOUT = 500;
        private const int TOTAL_PLAYERS_NEEDED_1 = 1;
        private const int TOTAL_PLAYERS_NEEDED_2 = 2;

        private const int MM_TICK = 1000;
        
        private MmApplication _mmApplication;
        private GameApplication _gameApplication;

        private TestClientPeer _client1, _client2, _client3;

        [SetUp]
        public void Setup()
        {
            // _measures = new Dictionary<byte, object>();
            // _measures.Add(PropertyCode.PlayerProperties.Level, 1);
            // _roomPropertiesProvider = new FakeRoomPropertiesProvider1();
            //
            // var config = new MmApplicationConfig("", "127.0.0.1", new List<ushort> {MM_SERVER_PORT}, "", 120000, GameProject.DefaultGame, "", 7002);
            // taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            // requestSender = new FakeSender();
            // _serverProvider = new FakeMatchMakerServerInfoProvider(requestSender, "127.0.0.1", "222");
            //
            // _backendProvider = new BackendProvider(taskSchedulerFactory, config, requestSender, _serverLogger);
            // _packetSender = new PacketBatchSender(taskSchedulerFactory, config, _serverLogger);
            // _playerManager = new PlayersManager( Mock.Of<IMmMetrics>(), _serverLogger);
            // _mmRoomManager =
            //     new MM.Managers.RoomManager(_serverProvider, _serverLogger, taskSchedulerFactory);
            // _mmGroupManager = new MatchMakingGroupManager(_serverLogger, taskSchedulerFactory, _playerManager, _packetSender,  Mock.Of<IMmMetrics>(), _mmRoomManager, _roomPropertiesProvider, config);
            //
            // matchMaker = new MatchMaker(_serverLogger,  _packetSender,Mock.Of<IMmMetrics>(), _playerManager, _mmGroupManager);
            // _measures = new Dictionary<byte, object>();
            // _measures.Add(PropertyCode.PlayerProperties.Level, 1);
            //
            // //matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_1, MM_TICK, true, true, 5000, 120000, new Dictionary<byte, object>(), new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 1}});
            // matchMaker.AddMatchMakingGroup(_measures);
            //
            // _measures = new Dictionary<byte, object>();
            // _measures.Add(PropertyCode.PlayerProperties.Level, 2);
            //
            // //matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_2, MM_TICK, true, true, 5000, 120000, new Dictionary<byte, object>(), new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 2}});
            // matchMaker.AddMatchMakingGroup(_measures);
            //
            // _measures = new Dictionary<byte, object>();
            // _measures.Add(PropertyCode.PlayerProperties.Level, 3);
            //
            // //matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_2, MM_TICK, true, true, 1000, 10000, new Dictionary<byte, object>(), new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 3}});
            // matchMaker.AddMatchMakingGroup(_measures);
            //
            // matchMaker.AddRequiredProperty(PropertyCode.PlayerProperties.Level);
            //
            // //setup server
            // _mmApplication = new MmApplication(_serverLogger, config, serializer, socketFactory, matchMaker,
            //     requestSender, taskSchedulerFactory, _backendProvider, _packetSender, _serverProvider, _mmRoomManager,
            //     _mmGroupManager, _playerManager, Mock.Of<IMmMetrics>());

            _gameApplication = InstanceHelper.GetGame(SERVER_PORT);
            _mmApplication = InstanceHelper.GetMm(MM_SERVER_PORT, 0, _gameApplication);
            _mmApplication.Start();
            _gameApplication.Start();
            
            //setup client
            _client1 = new TestClientPeer(_clientLogger, taskSchedulerFactory, serializer);
            _client2 = new TestClientPeer(_clientLogger, taskSchedulerFactory, serializer);
            _client3 = new TestClientPeer(_clientLogger, taskSchedulerFactory, serializer);
            
        }

        [TearDown]
        public void TearDown()
        {            
            if (_client1.IsConnected())
                _client1.Disconnect();
            if (_client2.IsConnected())
                _client2.Disconnect();
            if (_client3.IsConnected())
                _client3.Disconnect();
            _mmApplication.ShutDown();
            _gameApplication.ShutDown();
        }

        private void RegisterServer()
        {
            //actualize server
        }
    
        private async Task JoinMm(int level)
        {
            RegisterServer();
            //player join
            _client1.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            await _client1.WaitFor<ConnectedEvent>(e => true);
            
            //incorrect mm request
            var mmResponse = await _client1.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object>()));
            Assert.AreEqual(MatchMakingErrorCode.RequiredPlayerPropertyIsNotSet, mmResponse.MatchMakingErrorCode);
            
            //correct mm request
            mmResponse = await _client1.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, level} }));
        }
        
        [Test]
        public void RegisterAndActualizeServer()
        {
            //TODO rework
        }
        

        //player joins to first MM grou and gets JoinInfo
        [Test]
        public async Task SuccessfulJoin()
        {
            await JoinMm(1);
            var stats = _mmApplication.GetStats();

            await _client1.WaitFor<JoinInfoEvent>(e => true);
            
            await _client1.Send<LeaveMatchMakingResponse>(new LeaveMatchMakingRequest());
            stats = _mmApplication.GetStats();
            Assert.AreEqual(0, stats.TotalPlayers);
        }
        
        
        // //player joining with level which is not included in matchmaking rules
        // [Test]
        // public void UnsuccessfulJoinBecauseOfIncorrectLevel()
        // {
        //
        //     //11 level is not included in MM rules 
        //     JoinMm(11);
        //     var stats = _mmApplication.GetStats();
        //     
        //     //wait for MM_TICK*2 ms
        //     EmptyTask.Wait(MM_TICK*2);
        //     //check if we received join info event
        //     var joinInfoCount = _client1.GetCountOf(CustomOperationCode.JoinInfo);
        //     Assert.AreEqual(0, joinInfoCount);
        //     
        //     _client1.Send(new LeaveMatchMakingRequest());
        //     EmptyTask.Wait(WAIT_TIMEOUT*2);
        //     stats = _mmApplication.GetStats();
        //     Assert.AreEqual(0, stats.TotalPlayers);
        // }
        
        //player joining to group where 2 players are required
        [Test]
        public async Task UnsuccessfulLongMMTime()
        {            
            //11 level is not included in MM rules
            await JoinMm(1);
            var stats = _mmApplication.GetStats();

            await _client1.WaitFor<JoinInfoEvent>(e => true);
            await _client1.Send<LeaveMatchMakingResponse>(new LeaveMatchMakingRequest());
            stats = _mmApplication.GetStats();
            Assert.AreEqual(0, stats.TotalPlayers);
        }
        
        [Test]
        public async Task TwoPlayersJoinToSecondGroup()
        {
            RegisterServer();
            //clients connect
            _client1.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            _client2.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            await _client1.WaitFor<ConnectedEvent>(@event => true);
            await _client2.WaitFor<ConnectedEvent>(@event => true);
         
            //entering mm
            await _client1.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 2} }));
            await _client2.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 2} }));

            await _client1.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null);
            await _client2.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null);

            Assert.IsTrue(_client1.GetJoinInfo() != null && _client2.GetJoinInfo() != null && _client1.GetJoinInfo().Status == JoinStatus.RoomIsReady && _client2.GetJoinInfo().Status == JoinStatus.RoomIsReady);
            
        }

        [Test]
        public async Task SecondJoinToSameRoom()
        {
            RegisterServer();
            //clients connect
            _client1.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            _client2.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            _client3.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);

            EmptyTask.Wait(WAIT_TIMEOUT);
          

            EmptyTask.Wait(WAIT_TIMEOUT);
            //first creates room
            await _client1.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 3} }));
            //wait for adding bots and creating room
            EmptyTask.Wait(1000);
            //second should go to the same room
            await _client2.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 3} }));

            await _client1.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady);
            await _client2.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady);
            
            //check room number
            var stats = _mmApplication.GetStats();
            Assert.AreEqual(1, stats.CreatedRoomsCount);
            
            //third join after room closed
            EmptyTask.Wait(10000);
            await _client3.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 3} }));

            await _client3.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady);
            
            stats = _mmApplication.GetStats();
            Assert.AreEqual(2, stats.CreatedRoomsCount);

        }
    }
}