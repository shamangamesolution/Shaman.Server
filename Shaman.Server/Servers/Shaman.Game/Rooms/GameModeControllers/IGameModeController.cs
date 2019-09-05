using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;

namespace Shaman.Game.Rooms.GameModeControllers
{
    public interface IGameModeController
    {
        void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties);
        void CleanupPlayer(Guid sessionId);
        bool ProcessMessage(MessageBase message, Guid sessionId);
    }
}