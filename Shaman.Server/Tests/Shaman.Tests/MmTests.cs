using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Servers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Game.Providers;
using Shaman.MM;
using Shaman.MM.Configuration;
using Shaman.MM.MatchMaking;
using Shaman.MM.Players;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.Entity;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.Tests.Providers;
using Shaman.TestTools.ClientPeers;

namespace Shaman.Tests
{    
    [TestFixture]
    public class MmTests : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort MM_SERVER_PORT = 23450;
        private const ushort SERVER_PORT = 23451;
        private const ushort WAIT_TIMEOUT = 500;
        private const ushort TOTAL_PLAYERS_NEEDED_1 = 1;
        private const ushort TOTAL_PLAYERS_NEEDED_2 = 2;

        private const ushort MM_TICK = 1000;
        
        private MmApplication _mmApplication;

        private TestClientPeer _client1, _client2, _client3;

        private IPlayerCollection playerCollection = null; 
        private IMatchMaker matchMaker;
        private List<MatchMakingGroup> matchMakingGroups = new List<MatchMakingGroup>();
        private IRequestSender requestSender = null;
        private IBackendProvider _backendProvider;
        private IPacketSender _packetSender;

        private IMatchMakerServerInfoProvider _serverProvider;
        [SetUp]
        public void Setup()
        {             
            var config = new MmApplicationConfig("", "127.0.0.1", new List<ushort> {MM_SERVER_PORT}, "", 120000, 120000, GameProject.DefaultGame, "");
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            requestSender = new FakeSender();
            _serverProvider = new FakeMatchMakerServerInfoProvider(requestSender, "127.0.0.1", "222");
            
            _backendProvider = new BackendProvider(taskSchedulerFactory, config, requestSender, _serverLogger);
            playerCollection = new PlayerCollection(_serverLogger, Mock.Of<IMmMetrics>());
            _packetSender = new PacketBatchSender(taskSchedulerFactory, config, serializerFactory);
            var createdRoomManager = new CreatedRoomManager(taskSchedulerFactory, _serverLogger);
            matchMaker = new MatchMaker(playerCollection, _serverLogger, taskSchedulerFactory, serializerFactory, _packetSender,Mock.Of<IMmMetrics>(), createdRoomManager, _serverProvider);
            matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_1, MM_TICK, true, true, 5000, 120000, new Dictionary<byte, object>(), new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 1}});
            matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_2, MM_TICK, true, true, 5000, 120000, new Dictionary<byte, object>(), new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 2}});
            matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_2, MM_TICK, true, true, 1000, 10000, new Dictionary<byte, object>(), new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 3}});

            //setup server
            _mmApplication = new MmApplication(_serverLogger, config, serializerFactory, socketFactory, playerCollection, matchMaker,requestSender, taskSchedulerFactory, _backendProvider, _packetSender, createdRoomManager, _serverProvider);
            _mmApplication.SetMatchMakerProperties(new List<byte> {PropertyCode.PlayerProperties.Level});

            _mmApplication.Start();
            
            //setup client
            _client1 = new TestClientPeer(_clientLogger, taskSchedulerFactory);
            _client2 = new TestClientPeer(_clientLogger, taskSchedulerFactory);
            _client3 = new TestClientPeer(_clientLogger, taskSchedulerFactory);
            
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

        }

        private void RegisterServer()
        {
            //actualize server
        }
    
        private void JoinMm(int level)
        {

            RegisterServer();
            //player join
            _client1.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            EmptyTask.Wait(WAIT_TIMEOUT);
            _client1.Send(new AuthorizationRequest(1, Guid.NewGuid()));            
            EmptyTask.Wait(WAIT_TIMEOUT);
            Assert.AreEqual(1, _client1.GetCountOfSuccessResponses(CustomOperationCode.Authorization));
            
            //incorrect mm request
            _client1.Send(new EnterMatchMakingRequest(new Dictionary<byte, object>()));
            EmptyTask.Wait(WAIT_TIMEOUT);
            var mmResponse = _client1.GetMessageList().FirstOrDefault(m => m.OperationCode == CustomOperationCode.EnterMatchMaking) as EnterMatchMakingResponse;
            Assert.NotNull(mmResponse);
            Assert.AreEqual(MatchMakingErrorCode.RequiredPlayerPropertyIsNotSet, mmResponse.MatchMakingErrorCode);
            
            //correct mm request
            _client1.Send(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, level} }));
            EmptyTask.Wait(WAIT_TIMEOUT*2);
            var responses = _client1.GetMessageList().Where(m => m.OperationCode == CustomOperationCode.EnterMatchMaking).Select(r => r as EnterMatchMakingResponse);
            Assert.AreEqual(2, responses.Count());
            bool correctResponseFound = false;
            foreach (var response in responses)
            {
                if (response != null && response.MatchMakingErrorCode == MatchMakingErrorCode.OK)
                    correctResponseFound = true;
            }
            Assert.IsTrue(correctResponseFound);
        }
        
        [Test]
        public void RegisterAndActualizeServer()
        {
            //TODO rework
        }
        

        //player joins to first MM grou and gets JoinInfo
        [Test]
        public void SuccessfulJoin()
        {

            JoinMm(1);
            var stats = _mmApplication.GetStats();
            
            //wait for MM_TICK*2 ms
            EmptyTask.Wait(MM_TICK*2);
            //check if we received join info event
            var joinInfoCount = _client1.GetCountOf(CustomOperationCode.JoinInfo);
            Assert.AreEqual(1, joinInfoCount);
            
            _client1.Send(new LeaveMatchMakingRequest());
            EmptyTask.Wait(WAIT_TIMEOUT*2);
            stats = _mmApplication.GetStats();
            Assert.AreEqual(0, stats.TotalPlayers);
        }
        
        
        //player joining with level which is not included in matchmaking rules
        [Test]
        public void UnsuccessfulJoinBecauseOfIncorrectLevel()
        {

            //11 level is not included in MM rules 
            JoinMm(11);
            var stats = _mmApplication.GetStats();
            
            //wait for MM_TICK*2 ms
            EmptyTask.Wait(MM_TICK*2);
            //check if we received join info event
            var joinInfoCount = _client1.GetCountOf(CustomOperationCode.JoinInfo);
            Assert.AreEqual(0, joinInfoCount);
            
            _client1.Send(new LeaveMatchMakingRequest());
            EmptyTask.Wait(WAIT_TIMEOUT*2);
            stats = _mmApplication.GetStats();
            Assert.AreEqual(0, stats.TotalPlayers);
        }
        
        //player joining to group where 2 players are required
        [Test]
        public void UnsuccessfulLongMMTime()
        {            
            //11 level is not included in MM rules
            JoinMm(1);
            var stats = _mmApplication.GetStats();
            //wait for MM_TICK*2 ms
            EmptyTask.Wait(MM_TICK*3);
            //check if we received join info event
            var joinInfoCount = _client1.GetCountOf(CustomOperationCode.JoinInfo);
            Assert.IsTrue(joinInfoCount > 0);
            _client1.Send(new LeaveMatchMakingRequest());
            EmptyTask.Wait(WAIT_TIMEOUT*2);
            stats = _mmApplication.GetStats();
            Assert.AreEqual(0, stats.TotalPlayers);
        }
        
        [Test]
        public void TwoPlayersJoinToSecondGroup()
        {
            Task emptyTask = new Task(() => {});

            RegisterServer();
            //clients connect
            _client1.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            _client2.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            
            emptyTask.Wait(WAIT_TIMEOUT);
            //auth
            _client1.Send(new AuthorizationRequest(1, Guid.NewGuid()));         
            _client2.Send(new AuthorizationRequest(1, Guid.NewGuid()));            
            emptyTask.Wait(WAIT_TIMEOUT);
            //entering mm
            _client1.Send(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 2} }));
            _client2.Send(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 2} }));

            emptyTask.Wait(WAIT_TIMEOUT*2);
            
            var responsesCount = _client1.GetCountOf(CustomOperationCode.EnterMatchMaking) + _client2.GetCountOf(CustomOperationCode.EnterMatchMaking);
            Assert.AreEqual(2, responsesCount);

            //wait for MM_TICK*2 ms
            emptyTask.Wait(MM_TICK*2);
            //check if we received join info event
//            var joinInfoCount = _client1.GetCountOf(CustomOperationCode.JoinInfo) + _client2.GetCountOf(CustomOperationCode.JoinInfo);
//            Assert.AreEqual(2, joinInfoCount);

            Assert.IsTrue(_client1.GetJoinInfo() != null && _client2.GetJoinInfo() != null && _client1.GetJoinInfo().Status == JoinStatus.RoomIsReady && _client2.GetJoinInfo().Status == JoinStatus.RoomIsReady);
            
        }

        [Test]
        public void SecondJoinToSameRoom()
        {
            RegisterServer();
            //clients connect
            _client1.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            _client2.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            _client3.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);

            EmptyTask.Wait(WAIT_TIMEOUT);
            //auth
            _client1.Send(new AuthorizationRequest(1, Guid.NewGuid()));         
            _client2.Send(new AuthorizationRequest(1, Guid.NewGuid()));
            _client3.Send(new AuthorizationRequest(1, Guid.NewGuid()));            

            EmptyTask.Wait(WAIT_TIMEOUT);
            //entering mm
            //first creates room
            _client1.Send(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 3} }));
            //wait for adding bots and creating room
            EmptyTask.Wait(1000);
            //second should go to the same room
            _client2.Send(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 3} }));
            EmptyTask.Wait(WAIT_TIMEOUT*2);
            var responsesCount = _client1.GetCountOf(CustomOperationCode.EnterMatchMaking) + _client2.GetCountOf(CustomOperationCode.EnterMatchMaking);
            Assert.AreEqual(2, responsesCount);
            EmptyTask.Wait(MM_TICK*2);
            Assert.IsTrue(_client1.GetJoinInfo() != null && _client2.GetJoinInfo() != null && _client1.GetJoinInfo().Status == JoinStatus.RoomIsReady && _client2.GetJoinInfo().Status == JoinStatus.RoomIsReady);
            
            //check room number
            var stats = _mmApplication.GetStats();
            Assert.AreEqual(1, stats.CreatedRoomsCount);
            
            //third join after room closed
            EmptyTask.Wait(10000);
            _client3.Send(new EnterMatchMakingRequest(new Dictionary<byte, object> { {PropertyCode.PlayerProperties.Level, 3} }));
            EmptyTask.Wait(WAIT_TIMEOUT*2);
            responsesCount = _client1.GetCountOf(CustomOperationCode.EnterMatchMaking) + _client2.GetCountOf(CustomOperationCode.EnterMatchMaking) + _client3.GetCountOf(CustomOperationCode.EnterMatchMaking);
            Assert.AreEqual(3, responsesCount);
            EmptyTask.Wait(MM_TICK*2);
            Assert.IsTrue(_client3.GetJoinInfo() != null && _client3.GetJoinInfo().Status == JoinStatus.RoomIsReady);
            stats = _mmApplication.GetStats();
            Assert.AreEqual(2, stats.CreatedRoomsCount);

        }
    }
}