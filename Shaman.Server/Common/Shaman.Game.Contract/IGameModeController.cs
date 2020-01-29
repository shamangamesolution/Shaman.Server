using System;
using System.Collections.Generic;
using Shaman.Messages.Handling;

namespace Shaman.Game.Contract
{
    public interface IGameModeController
    {
        void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties);
        void CleanupPlayer(Guid sessionId);
        bool IsGameFinished();
        TimeSpan GetGameTtl();
        void Cleanup();
        void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId);
    }
}