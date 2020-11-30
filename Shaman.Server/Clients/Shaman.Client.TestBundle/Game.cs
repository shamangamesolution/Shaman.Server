using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;

namespace Shaman.Client.TestBundle
{
    public class Game:IGameBundle
    {
        public IGameModeControllerFactory GetGameModeControllerFactory()
        {
            throw new System.NotImplementedException();
        }

        public void OnInitialize(IShamanComponents shamanComponents)
        {
            throw new System.NotImplementedException();
        }
    }

    class MyClass:IGameModeControllerFactory
    {
        public IGameModeController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new TestGameController();
        }
    }

    class TestGameController:IGameModeController
    {
        public Task<bool> ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            return Task.FromResult(true);
        }

        public void CleanupPlayer(Guid sessionId, PeerDisconnectedReason reason, byte[] reasonPayload)
        {
        }

        public bool IsGameFinished()
        {
            return false;
        }

        public TimeSpan ForceDestroyRoomAfter => TimeSpan.FromMinutes(5);
        public void Cleanup()
        {
        }

        public void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId)
        {
        }
    }
}