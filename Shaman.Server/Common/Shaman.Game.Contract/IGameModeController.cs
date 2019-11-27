using System;
using System.Collections.Generic;
using Shaman.Common.Server.Handling;

namespace Shaman.Game.Contract
{
    public interface IGameModeController
    {
        void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties);
        void CleanupPlayer(Guid sessionId);
        bool IsGameFinished();
        TimeSpan GetGameTtl();
        void Cleanup();
        MessageResult ProcessMessage(MessageData message, Guid sessionId);
    }
}