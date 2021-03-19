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
            _gameApplication = InstanceHelper.GetGame(SERVER_PORT);
            _mmApplication = InstanceHelper.GetMm(MM_SERVER_PORT, 0, _gameApplication, 2, 500);
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
            EmptyTask.Wait(100);
            //second should go to the same room
            await _client2.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 3} }));

            await _client1.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady);
            await _client2.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady);
            
            //check room number
            var stats = _mmApplication.GetStats();
            Assert.AreEqual(1, stats.CreatedRoomsCount);
            
            //third join after room closed
            await _client3.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 3} }));

            await _client3.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady, 11000);
            
            stats = _mmApplication.GetStats();
            Assert.AreEqual(2, stats.CreatedRoomsCount);

        }

        [Test]
        public async Task JoinMaxPlusOne()
        {
            RegisterServer();
            
            //clients connect
            _client1.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            _client2.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);
            _client3.Connect(CLIENT_CONNECTS_TO_IP, MM_SERVER_PORT);

            EmptyTask.Wait(WAIT_TIMEOUT);

            await _client1.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 3} }));
            await _client2.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 3} }));
            await _client3.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 3} }));
            
            //wait for creating room
            EmptyTask.Wait(1000);
            
            await _client1.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady);
            await _client2.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady);
            await _client3.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady);

            //check room number
            var stats = _mmApplication.GetStats();
            Assert.AreEqual(2, stats.CreatedRoomsCount);
        }
    }
}