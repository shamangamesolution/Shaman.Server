using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.MM;
using Shaman.MM.Configuration;
using Shaman.MM.MatchMaking;
using Shaman.MM.Players;
using Shaman.MM.Servers;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Tests.ClientPeers;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.Entity;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Shaman.Tests
{    
    [TestFixture]
    public class MmTests : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort MM_SERVER_PORT = 23450;
        private const ushort SERVER_PORT = 23451;
        private const ushort WAIT_TIMEOUT = 100;
        private const ushort TOTAL_PLAYERS_NEEDED_1 = 1;
        private const ushort TOTAL_PLAYERS_NEEDED_2 = 2;

        private const ushort MM_TICK = 1000;
        
        private MmApplication _mmApplication;

        private TestClientPeer _client1, _client2;

        private IRegisteredServerCollection serverCollection = null; 
        private IPlayerCollection playerCollection = null; 
        private IMatchMaker matchMaker;
        private List<MatchMakingGroup> matchMakingGroups = new List<MatchMakingGroup>();
        private IRequestSender requestSender = null;
        private IBackendProvider _backendProvider;
        private IPacketSender _packetSender;
        [SetUp]
        public void Setup()
        {             
            var config = new MmApplicationConfig("127.0.0.1", new ushort[] {MM_SERVER_PORT}, "", 120000, 120000, GameProject.DefaultGame, "", "");
            taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            requestSender = new FakeSender();

            _backendProvider = new BackendProvider(taskSchedulerFactory, config, requestSender, _serverLogger);
            playerCollection = new PlayerCollection(_serverLogger, serializerFactory);
            serverCollection = new RegisteredServersCollection(_serverLogger, config, taskSchedulerFactory);
            _packetSender = new PacketBatchSender(taskSchedulerFactory, config, serializerFactory);
            matchMaker = new MatchMaker(serverCollection, playerCollection, _serverLogger, taskSchedulerFactory, serializerFactory, _packetSender);
            matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_1, MM_TICK, true, true, 5000, new Dictionary<byte, object>(), new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 1}});
            matchMaker.AddMatchMakingGroup(TOTAL_PLAYERS_NEEDED_2, MM_TICK, true, true, 5000, new Dictionary<byte, object>(), new Dictionary<byte, object> {{PropertyCode.PlayerProperties.Level, 2}});

            //setup server
            _mmApplication = new MmApplication(_serverLogger, config, serializerFactory, socketFactory, serverCollection, playerCollection, matchMaker,requestSender, taskSchedulerFactory, _backendProvider, _packetSender);
            _mmApplication.SetMatchMakerProperties(new List<byte> {PropertyCode.PlayerProperties.Level});

            _mmApplication.Start();
            
            //setup client
            _client1 = new TestClientPeer(_clientLogger, taskSchedulerFactory);
            
            _client2 = new TestClientPeer(_clientLogger, taskSchedulerFactory);
        }

        [TearDown]
        public void TearDown()
        {            
            if (_client1.IsConnected())
                _client1.Disconnect();
            if (_client2.IsConnected())
                _client2.Disconnect();
            
            _mmApplication.ShutDown();

        }

        private void RegisterServer()
        {
            //actualize server
            EmptyTask.Wait(WAIT_TIMEOUT*2);
            _mmApplication.ActualizeServer(new ActualizeServerRequest(new ServerIdentity(CLIENT_CONNECTS_TO_IP, new List<ushort>() {MM_SERVER_PORT} ), "", new Dictionary<ushort, int> {{SERVER_PORT, 12}}));

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
            var stats = _mmApplication.GetStats();
            Assert.AreEqual(0, stats.RegisteredServers.Count());
            Assert.AreEqual(0, stats.TotalPlayers);
            Assert.IsNull(stats.OldestPlayerInMatchMaking);
            RegisterServer();
            stats = _mmApplication.GetStats();
            Assert.AreEqual(1, stats.RegisteredServers.Count());
            var server = stats.RegisteredServers.FirstOrDefault();
            Assert.NotNull(server);
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
        
    }
}