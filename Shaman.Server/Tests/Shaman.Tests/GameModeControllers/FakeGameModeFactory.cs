using System;
using System.Collections.Generic;
using System.Threading;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Messages;
using Shaman.Messages.RoomFlow;

namespace Shaman.Tests.GameModeControllers
{
    public class FakeGameModeController : IGameModeController
    {
        private readonly IRoom _room;
        private readonly ISerializer _serializer;

        private int _plyerCount = 0;

        public FakeGameModeController(IRoom room)
        {
            _room = room;
            _serializer = new BinarySerializer();
        }
        
        public void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            Interlocked.Increment(ref _plyerCount);
            _room.AddToSendQueue(new JoinRoomResponse(), sessionId);
            _room.ConfirmedJoin(sessionId);
        }

        public void CleanupPlayer(Guid sessionId)
        {
            Interlocked.Decrement(ref _plyerCount);
        }

        public bool ProcessMessage(MessageBase message, Guid sessionId)
        {
            return true;
        }

        public bool IsGameFinished()
        {
            return _plyerCount == 0;
        }

        public TimeSpan ForceDestroyRoomAfter => TimeSpan.MaxValue;

        public void Cleanup()
        {
        }

        public void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId)
        {
            //process room message
            switch (operationCode)
            {
                case CustomOperationCode.Test:
                    var testRoomEvent =
                        _serializer.DeserializeAs<TestRoomEvent>(message.Buffer, message.Offset, message.Length);
                    _room.SendToAll(testRoomEvent, new[] {sessionId});
                    break;
            }
        }
    }
    
    public class FakeGameModeControllerFactory : IGameModeControllerFactory
    {
        public IGameModeController GetGameModeController(IRoom room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new FakeGameModeController(room);

        }
    }
}