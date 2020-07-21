using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Common.Contract;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Messages;

namespace Shaman.Tests.GameModeControllers
{
    public class FakeGameModeController : IGameModeController
    {
        private readonly IRoomContext _room;
        private readonly ISerializer _serializer;

        private int _playerCount = 0;

        public FakeGameModeController(IRoomContext room)
        {
            _room = room;
            _serializer = new BinarySerializer();
        }

        public Task<bool> ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            Interlocked.Increment(ref _playerCount);
            return Task.FromResult(true);
        }

        public void CleanupPlayer(Guid sessionId, PeerDisconnectedReason reason, byte[] reasonPayload)
        {
            Interlocked.Decrement(ref _playerCount);
        }

        public bool IsGameFinished()
        {
            return _playerCount == 0;
        }

        public TimeSpan ForceDestroyRoomAfter => TimeSpan.MaxValue;

        public void Cleanup()
        {
        }

        public void ProcessMessage(MessageData message, DeliveryOptions deliveryOptions, Guid sessionId)
        {
            var testRoomEvent =
                _serializer.DeserializeAs<TestRoomEvent>(message.Buffer, message.Offset, message.Length);
            _room.SendToAll(message,
                new TransportOptions
                    {IsReliable = testRoomEvent.IsReliable, IsOrdered = testRoomEvent.IsOrdered}, sessionId);
        }
    }

    public class FakeGameModeControllerFactory : IGameModeControllerFactory
    {
        public IGameModeController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new FakeGameModeController(room);
        }
    }
}