using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Shaman.Common.Http;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Providers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.MM;
using Shaman.Contract.Routing.MM;
using Shaman.Game;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.MM;
using Shaman.MM.MatchMaking;
using Shaman.Tests.GameModeControllers;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.Tests.Helpers;
using Shaman.Tests.Providers;
using Shaman.TestTools.ClientPeers;
using Shaman.TestTools.Events;
using GameProject = Shaman.Messages.General.Entity.GameProject;
using IRoomManager = Shaman.Game.Rooms.IRoomManager;
using RoomManager = Shaman.Game.Rooms.RoomManager;

namespace Shaman.Tests
{
    [TestFixture]
    public class IntegrationTests : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort SERVER_PORT_GAME = 23451;
        private const ushort SERVER_PORT_MM = 23452;
        private const ushort WAIT_TIMEOUT = 1000;
        private const int MM_TICK = 250;
        private const int CLIENTS_NUMBER_1 = 12;
        private const int CLIENTS_NUMBER_2 = 100;
        
        private const int TOTAL_PLAYERS_NEEDED_1 = 12;
        private const int EVENTS_SENT = 10;
        
        private GameApplication _gameApplication;
        private MmApplication _mmApplication;

        private List<TestClientPeer> _clients = new List<TestClientPeer>();
        
        [SetUp]
        public void Setup()
        {             
            _clients.Clear();

            _gameApplication = InstanceHelper.GetGame(SERVER_PORT_GAME);
            _mmApplication = InstanceHelper.GetMm(SERVER_PORT_MM, SERVER_PORT_GAME, _gameApplication, 12);

            _mmApplication.Start();
            _gameApplication.Start();
        }
        
        [TearDown]
        public void TearDown()
        {
            _mmApplication.ShutDown();
            _gameApplication.ShutDown();
        }

        [Test]
        public void IntegrationTest()
        {
            var isSuccess = false;
            //create 12 clients and connect them to mm
            for (var i = 0; i < CLIENTS_NUMBER_1; i++)
            {
                var client = new TestClientPeer( _clientLogger, taskSchedulerFactory, serializer);
                client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM);
                _clients.Add(client);
                EmptyTask.Wait(WAIT_TIMEOUT);
            }
            
            //send auth
            _clients.ForEach(c => c.Send<AuthorizationResponse>(new AuthorizationRequest()).Wait());

            //send join matchmaking (with level = 1)
            _clients.ForEach(c => c.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 1} })).Wait());
            _clients.ForEach(c=>c.WaitFor<JoinInfoEvent>(e => e.JoinInfo != null && e.JoinInfo.Status == JoinStatus.RoomIsReady));

            var mmStats = _mmApplication.GetStats();
            Assert.AreEqual(1, mmStats.RegisteredServers.Count);
            
            //sending leave mathmaking request
            _clients.ForEach(c => c.Send<LeaveMatchMakingResponse>(new LeaveMatchMakingRequest()).Wait());
            _clients.ForEach(c => c.Disconnect());

            mmStats = _mmApplication.GetStats();
            Assert.AreEqual(0, mmStats.TotalPlayers);
            Assert.AreEqual(1, mmStats.RegisteredServers.Count);
            
            //connect to server
            _clients.ForEach(c => c.Connect(c.GetJoinInfo().ServerIpAddress.ToString(), c.GetJoinInfo().ServerPort));
            var roomId = _clients.First().GetJoinInfo().RoomId;
            EmptyTask.Wait(MM_TICK*2);
            var stats = _gameApplication.GetStats();
            Assert.AreEqual(CLIENTS_NUMBER_1, stats.PeerCount);
            Assert.AreEqual(1, stats.RoomCount);
            var roomsPeerCount = stats.RoomsPeerCount.First();
            //players did not join room yet
            Assert.AreEqual(roomId, roomsPeerCount.Key);
            Assert.AreEqual(0, roomsPeerCount.Value);
            
            //authing
            _clients.ForEach(c =>
            {
                var sessionId = Guid.NewGuid();
                c.SessionId = sessionId;
                c.Send<AuthorizationResponse>(new AuthorizationRequest() {SessionId = sessionId});
            });
            EmptyTask.Wait(WAIT_TIMEOUT);
            
            //joining room
            _clients.ForEach(c => c.Send<JoinRoomResponse>(new JoinRoomRequest(roomId, new Dictionary<byte, object>())));
            EmptyTask.Wait(WAIT_TIMEOUT);
            stats = _gameApplication.GetStats();
            roomsPeerCount = stats.RoomsPeerCount.First();
            
            //players joined room
            Assert.AreEqual(roomId, roomsPeerCount.Key);
            Assert.AreEqual(CLIENTS_NUMBER_1, roomsPeerCount.Value);

            //activity
            //each client sends 1000 events to others
            _clients.ForEach(c =>
            {
                for (int i = 0; i < EVENTS_SENT; i++)
                {
                    c.SendEvent(new TestRoomEvent(true, 122, 4.668f, new List<int>()));
                }
            });
            
            EmptyTask.Wait(WAIT_TIMEOUT * 100);

            isSuccess = true;
            _clients.ForEach(c =>
            {
                if (c.CountOf<TestRoomEvent>() != (CLIENTS_NUMBER_1 - 1) * EVENTS_SENT)
                {
                    _clientLogger.Error($"test events {c.CountOf<TestRoomEvent>()}/{(CLIENTS_NUMBER_1 - 1) * EVENTS_SENT}");
                    isSuccess = false;
                }
            });
            Assert.IsTrue(isSuccess);
            
            //disconnect from server
            _clients.ForEach(c => c.Disconnect());
            EmptyTask.Wait(WAIT_TIMEOUT);
            stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);

        }

        [Test]
        public void LotsOfClientsGoToGameServer()
        {
            var isSuccess = false;
            //create N clients and connect them to mm
            for (var i = 0; i < CLIENTS_NUMBER_2; i++)
            {
                var client = new TestClientPeer(_clientLogger, taskSchedulerFactory, serializer);
                client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT_MM);
                _clients.Add(client);
                EmptyTask.Wait(WAIT_TIMEOUT);
            }
            
            //send auth
            _clients.ForEach(c => c.Send<AuthorizationResponse>(new AuthorizationRequest()).Wait());
            
            //send join matchmaking (with level = 1)
            _clients.ForEach(c => c.Send<EnterMatchMakingResponse>(new EnterMatchMakingRequest(new Dictionary<byte, object> { {FakePropertyCodes.PlayerProperties.Level, 1} })));
            
            EmptyTask.Wait(MM_TICK* (CLIENTS_NUMBER_2 / 8));
            //wait maximum mm time
            EmptyTask.Wait(6000);

            //check joininfo existance
            isSuccess = true;
            int notJoinedCount = 0;
            int joinedCount = 0;
            _clients.ForEach(c =>
            {
                if (c.GetJoinInfo() == null || c.GetJoinInfo().Status != JoinStatus.RoomIsReady)
                {
                    if (c.GetJoinInfo() != null)
                        _clientLogger.Info($"Checking joinInfo. Status = {c.GetJoinInfo().Status}");
                    notJoinedCount++;
                    isSuccess = false;
                }
                else
                {
                    if (c.GetJoinInfo() != null)
                        _clientLogger.Info($"Checking joinInfo. Status = {c.GetJoinInfo().Status}");
                    joinedCount++;
                }
            });
            var roomsCount = CLIENTS_NUMBER_2 / TOTAL_PLAYERS_NEEDED_1 + (CLIENTS_NUMBER_2 % TOTAL_PLAYERS_NEEDED_1 > 0 ? 1: 0);
            
            Assert.AreEqual(CLIENTS_NUMBER_2 - joinedCount, notJoinedCount);
            
            var mmStats = _mmApplication.GetStats();
            Assert.AreEqual(1, mmStats.RegisteredServers.Count);
            
            //sending leave mathmaking request
            _clients.ForEach(c => c.Send<LeaveMatchMakingResponse>(new LeaveMatchMakingRequest()));
            _clients.ForEach(c => c.Disconnect());

            mmStats = _mmApplication.GetStats();
            Assert.AreEqual(0, mmStats.TotalPlayers);
            Assert.AreEqual(1, mmStats.RegisteredServers.Count);
            
            //connect to server
            _clients.Where(c => c.GetJoinInfo() != null && c.GetJoinInfo().Status == JoinStatus.RoomIsReady).ToList().ForEach(c => c.Connect(c.GetJoinInfo().ServerIpAddress.ToString(), c.GetJoinInfo().ServerPort));
            EmptyTask.Wait(MM_TICK*10);
            var stats = _gameApplication.GetStats();
            Assert.AreEqual(joinedCount, stats.PeerCount);
            Assert.AreEqual(roomsCount, stats.RoomCount);
            
            //authing
            _clients.ForEach(c => c.Send<AuthorizationResponse>(new AuthorizationRequest()));
            EmptyTask.Wait(WAIT_TIMEOUT);
            
            //joining room
            _clients.Where(c => c.GetJoinInfo() != null).ToList().ForEach(c => c.Send<JoinRoomResponse>(new JoinRoomRequest(c.GetJoinInfo().RoomId, new Dictionary<byte, object>())));
            EmptyTask.Wait(WAIT_TIMEOUT * 2);
            stats = _gameApplication.GetStats();
            Assert.AreEqual(roomsCount, stats.RoomCount);
            Assert.AreEqual(stats.RoomsPeerCount.Count, stats.RoomCount);

            //disconnect from server
            _clients.ForEach(c => c.Disconnect());
            EmptyTask.Wait(WAIT_TIMEOUT);
            stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);
        }
    }
}