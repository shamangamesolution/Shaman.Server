using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Messages.RoomFlow;

namespace Shaman.Game.Rooms.GameModeControllers
{
    public class TestModeController : IGameModeController
    {
        private IRoom _room;

        public TestModeController(IRoom room)
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
}