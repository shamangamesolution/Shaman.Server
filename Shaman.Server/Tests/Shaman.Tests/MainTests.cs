using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shaman.Common.Http;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Game;
using Shaman.Game.Metrics;
using Shaman.Game.Rooms;
using Shaman.Tests.GameModeControllers;
using Shaman.Tests.Helpers;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.RoomFlow;
using Shaman.TestTools.ClientPeers;
using Shaman.TestTools.Events;

namespace Shaman.Tests
{
    [TestFixture]
    public class MainTests : TestSetBase
    {
        private const string CLIENT_CONNECTS_TO_IP = "127.0.0.1";
        private const ushort SERVER_PORT = 23450;
        private const ushort WAIT_TIMEOUT = 300;
        
        private GameApplication _gameApplication;
        private IPEndPoint _ep = _testEndPoint;
        private TestClientPeer _client;           
        private static readonly IPEndPoint _testEndPoint;

        static MainTests()
        {
            _testEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
        }

        [SetUp]
        public void Setup()
        {
            //
            // var config = new GameApplicationConfig("", "", "127.0.0.1", new List<ushort> {SERVER_PORT}, "", "", 7000);
            // taskSchedulerFactory = new TaskSchedulerFactory(_serverLogger);
            //
            // _requestSender = new FakeSender();
            //
            // _backendProvider = new BackendProvider(taskSchedulerFactory, config, _requestSender, _serverLogger);
            //
            // emptyTask.Wait(2000);
            //
            // //setup server
            // _roomControllerFactory = new FakeRoomControllerFactory();
            // _packetSender = new PacketBatchSender(taskSchedulerFactory, config, _serverLogger);
            // _roomManager = new RoomManager(_serverLogger, serializer, config, taskSchedulerFactory,  _roomControllerFactory, _packetSender, Mock.Of<IGameMetrics>(), _requestSender);
            // _gameApplication = new GameApplication(_serverLogger, config, serializer, socketFactory, taskSchedulerFactory, _requestSender, _backendProvider, _roomManager, _packetSender);

            _gameApplication = InstanceHelper.GetGame(SERVER_PORT, true);
            _gameApplication.Start();
            
            //setup client
            _client = new TestClientPeer(_clientLogger, taskSchedulerFactory, serializer);
            
            
        }

        [TearDown]
        public void TearDown()
        {
            _gameApplication.ShutDown();
        }
        
        

        
        [Test]
        public void NewPeerConnectTest()
        {
            _gameApplication.GetListeners()[0].OnNewClientConnect(_ep);
            var peer = _gameApplication.GetListeners()[0].GetPeerCollection().Get(_ep);
            Assert.NotNull(peer);
        }
        
        [Test]
        public void TestAuth()
        {
            _gameApplication.GetListeners()[0].OnNewClientConnect(_ep);
            var peer = _gameApplication.GetListeners()[0].GetPeerCollection().Get(_ep);
            Assert.False(peer.IsAuthorized);
            var packetInfo = PackageHelper.GetPacketInfo(new AuthorizationRequest());
            _gameApplication.GetListeners()[0].OnReceivePacketFromClient(_testEndPoint, packetInfo);
            Assert.True(peer.IsAuthorized);
        }
        
        [Test]
        public void TestDirectJoinLeftRoom()
        {
            //stats
            var stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);
            Assert.AreEqual(0, stats.RoomCount);            
            
            //create room
            var roomId = _gameApplication.CreateRoom(
                new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.SinglePlayer}},
                new Dictionary<Guid, Dictionary<byte, object>>());
            
            stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);
            Assert.AreEqual(1, stats.RoomCount); 
            
            //join new player
            _gameApplication.GetListeners()[0].OnNewClientConnect(_ep);
            stats = _gameApplication.GetStats();
            Assert.AreEqual(1, stats.PeerCount);
            Assert.AreEqual(1, stats.RoomCount); 
            
            var peer = _gameApplication.GetListeners()[0].GetPeerCollection().Get(_ep);
            //check peer is not authorized
            Assert.False(peer.IsAuthorized);
            //sen JoinRoom Message
            _gameApplication.GetListeners()[0].OnReceivePacketFromClient(_testEndPoint, PackageHelper.GetPacketInfo(new JoinRoomRequest(roomId, new Dictionary<byte, object>())));
            //get room by peerId
            var room = _gameApplication.GetRoomManager().GetRoomBySessionId(peer.GetSessionId());
            //check it is null
            Assert.Null(room);
            //now authorize
            _gameApplication.GetListeners()[0].OnReceivePacketFromClient(_testEndPoint, PackageHelper.GetPacketInfo(new AuthorizationRequest()));
            //check if we authorized
            Assert.True(peer.IsAuthorized);
            //join one more time
            _gameApplication.GetListeners()[0].OnReceivePacketFromClient(_testEndPoint, PackageHelper.GetPacketInfo(new JoinRoomRequest(roomId, new Dictionary<byte, object>())));
            //get room by peerId
            room = _gameApplication.GetRoomManager().GetRoomBySessionId(peer.GetSessionId());
            //room exists
            Assert.NotNull(room);
            //test stats
            stats = _gameApplication.GetStats();
            Assert.AreEqual(1, stats.PeerCount);
            Assert.AreEqual(1, stats.RoomCount);
            Assert.NotNull(stats.RoomsPeerCount.FirstOrDefault());
            Assert.AreEqual(room.GetRoomId(), stats.RoomsPeerCount.FirstOrDefault().Key);
            Assert.AreEqual(1, stats.RoomsPeerCount.FirstOrDefault().Value);

            //leave
            _gameApplication.GetListeners()[0].OnReceivePacketFromClient(_testEndPoint, PackageHelper.GetPacketInfo(new LeaveRoomEvent()));
            //get room by peerId
            room = _gameApplication.GetRoomManager().GetRoomBySessionId(peer.GetSessionId());
            //assert room is null
            Assert.Null(room);
            
            //test stats once again
            stats = _gameApplication.GetStats();
            Assert.AreEqual(1, stats.PeerCount);
            Assert.AreEqual(0, stats.RoomCount);
            
            //disconnect
            _gameApplication.GetListeners()[0].OnClientDisconnect(_ep, Mock.Of<IDisconnectInfo>());
            stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);
            Assert.AreEqual(0, stats.RoomCount);           

        }

        [Test]
        public async Task NotAuthentificatedReceivedTest()
        {
            //check stats
            var stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);
            Assert.AreEqual(0, stats.RoomCount);
            
            _client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT);
 
            EmptyTask.Wait(WAIT_TIMEOUT);
            //create room
            var roomId = _gameApplication.CreateRoom(
                new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.SinglePlayer}},
                new Dictionary<Guid, Dictionary<byte, object>>());
            
            //try to send something without auth
            _client.Send(new JoinRoomRequest(roomId, new Dictionary<byte, object>()));
            await _client.WaitFor<AuthorizationResponse>(resp => !resp.Success);
            
            _client.Send(new JoinRoomRequest(roomId, new Dictionary<byte, object> {{FakePropertyCodes.PlayerProperties.BackendId, 1}}));
            await _client.WaitFor<AuthorizationResponse>(resp => !resp.Success);

            //authing
            await _client.Send<AuthorizationResponse>(new AuthorizationRequest());            

            //send again
            await _client.Send<JoinRoomResponse>(new JoinRoomRequest(roomId, new Dictionary<byte, object>()));
            
            //check stats
            stats = _gameApplication.GetStats();
            Assert.AreEqual(1, stats.PeerCount);
            Assert.AreEqual(1, stats.RoomCount);
            
            //disconnect
            _client.Disconnect();
            EmptyTask.Wait(2000);
            
            stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);
            Assert.AreEqual(0, stats.RoomCount);
        }
        
        [Test]
        public void ClientConnectTest()
        {
            _client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT);                
            EmptyTask.Wait(WAIT_TIMEOUT);

            Assert.AreEqual(1, _gameApplication.GetStats().PeerCount);
            _client.Disconnect();
            EmptyTask.Wait(WAIT_TIMEOUT);

            Assert.AreEqual(0, _gameApplication.GetStats().PeerCount);
            _client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT);     
            EmptyTask.Wait(WAIT_TIMEOUT);

            Assert.AreEqual(1, _gameApplication.GetStats().PeerCount);
            
            _client.Disconnect();
            EmptyTask.Wait(WAIT_TIMEOUT);

        }

        [Test]
        public void PassProperties()
        {
           
            var roomId = _gameApplication.CreateRoom(
                new Dictionary<byte, object>() {{PropertyCode.RoomProperties.GameMode, (byte) GameMode.SinglePlayer}},
                new Dictionary<Guid, Dictionary<byte, object>>());
            
            _client.Connect(CLIENT_CONNECTS_TO_IP, SERVER_PORT);                
            EmptyTask.Wait(WAIT_TIMEOUT);
            
            //authing
            _client.Send(new AuthorizationRequest());            
            EmptyTask.Wait(WAIT_TIMEOUT);
            
            //send join
            var props = new Dictionary<byte, object>
            {
                {1, 100},
                {2, (byte) 101},
                {3, (short) 102},
                {4, (ushort) 103},
                {5, (uint) 104},
                {6, (float) 105.15},
                {7, true},
                {8, (long) 106},
                {9, (ulong) 110},
                {10, new byte[] {111, 112, 113}}
            };
            _client.Send(new JoinRoomRequest(roomId, props));
            EmptyTask.Wait(WAIT_TIMEOUT);


            var peer = _gameApplication.GetListeners()[0].GetPeerCollection().GetAll().FirstOrDefault().Value;
            var room = _gameApplication.GetRoomManager().GetRoomBySessionId(peer.GetSessionId());
            var roomPlayer = room.FindPlayer(peer.GetSessionId());

            Assert.AreEqual(props, roomPlayer.Properties);
        }
    }
}