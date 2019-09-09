using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Rooms;
using Shaman.Game.Rooms.GameModeControllers;
using Shaman.Messages.General.Entity;
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
    }
    
    public class FakeGameModeControllerFactory : IGameModeControllerFactory
    {
        public IGameModeController GetGameModeController(GameMode mode, IRoom room, ITaskScheduler taskScheduler)
        {
            return new FakeGameModeController(room);
        }
    }
}