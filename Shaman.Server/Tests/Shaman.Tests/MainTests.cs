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
            _gameApplication = InstanceHelper.GetGame(SERVER_PORT);
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
            //join 
            _gameApplication.GetListeners()[0].OnReceivePacketFromClient(_testEndPoint, PackageHelper.GetPacketInfo(new JoinRoomRequest(roomId, new Dictionary<byte, object>())));
            //get room by peerId
            var room = _gameApplication.GetRoomManager().GetRoomBySessionId(peer.GetSessionId());
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
            
            //disconnect
            _gameApplication.GetListeners()[0].OnClientDisconnect(_ep, Mock.Of<IDisconnectInfo>());
            stats = _gameApplication.GetStats();
            Assert.AreEqual(0, stats.PeerCount);

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
            _client.Send<JoinRoomResponse>(new JoinRoomRequest(roomId, props));

            EmptyTask.Wait(WAIT_TIMEOUT);


            var peer = _gameApplication.GetListeners()[0].GetPeerCollection().GetAll().FirstOrDefault().Value;
            var room = _gameApplication.GetRoomManager().GetRoomBySessionId(peer.GetSessionId());
            var roomPlayer = room.FindPlayer(peer.GetSessionId());

            Assert.AreEqual(props, roomPlayer.Properties);
        }
    }
}