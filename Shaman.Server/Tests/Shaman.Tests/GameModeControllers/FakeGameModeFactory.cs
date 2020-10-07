using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;
using Shaman.Tests.Helpers;
using Shaman.TestTools.ClientPeers;
using Shaman.TestTools.Events;

namespace Shaman.Tests.GameModeControllers
{
    public class FakeRoomController : IRoomController
    {
        private readonly IRoomContext _room;
        private readonly ISerializer _serializer;
        private readonly ISendManager _sendManager;
        
        private int _playerCount = 0;
        
        public FakeRoomController(IRoomContext room)
        {
            _room = room;
            _serializer = new BinarySerializer();
            _sendManager = new SendManager(_room, _serializer);
        }

        public Task<bool> ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            Interlocked.Increment(ref _playerCount);
            return Task.FromResult(true);
        }

        public void ProcessPlayerDisconnected(Guid sessionId, PeerDisconnectedReason reason, byte[] reasonPayload)
        {
            Interlocked.Decrement(ref _playerCount);
        }

        public bool IsGameFinished()
        {
            //returning false to not allow room close
            return false;
        }

        public TimeSpan ForceDestroyRoomAfter => TimeSpan.MaxValue;

        public void ProcessMessage(Payload message, DeliveryOptions deliveryOptions, Guid sessionId)
        {
            // var testRoomEvent =
            //     _serializer.DeserializeAs<TestRoomEvent>(message.Buffer, message.Offset, message.Length);
            var operationCode = (byte)MessageBase.GetOperationCode(message.Buffer, message.Offset);

            if (operationCode == TestEventCodes.TestEventCode)
            {
                var deserializedMessage =
                    _serializer.DeserializeAs<TestRoomEvent>(message.Buffer, message.Offset, message.Length);
                _sendManager.SendToAll(deserializedMessage, sessionId);
            }
        }

        public int MaxMatchmakingWeight => 1;

        public void Dispose()
        {
        }
    }

    public class FakeRoomControllerFactory : IRoomControllerFactory
    {
        public FakeRoomControllerFactory()
        {
            
        }
        public IRoomController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new FakeRoomController(room);
        }
    }
}