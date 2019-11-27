using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.Handling;
using Shaman.Messages.RoomFlow;

namespace Shaman.Tests.GameModeControllers
{
    public class FakeGameModeController : IGameModeController
    {
        private IRoom _room;
        
        public FakeGameModeController(IRoom room)
        {
            _room = room;
        }
        
        public void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            _room.AddToSendQueue(new JoinRoomResponse(), sessionId);
            _room.ConfirmedJoin(sessionId);
        }

        public void CleanupPlayer(Guid sessionId)
        {
            
        }

        public bool ProcessMessage(MessageBase message, Guid sessionId)
        {
            return true;
        }

        public bool IsGameFinished()
        {
            return false;
        }

        public TimeSpan GetGameTtl()
        {
            return TimeSpan.Zero;
        }

        public void Cleanup()
        {
        }

        public MessageResult ProcessMessage(MessageData message, Guid sessionId)
        {
            return new MessageResult {Handled = false, DeserializedMessage = new PingRequest()};
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