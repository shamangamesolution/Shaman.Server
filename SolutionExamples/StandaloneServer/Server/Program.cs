using System;
using System.Collections.Generic;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game;
using Shaman.Game.Contract;
using Shaman.Messages.Handling;

namespace Server
{
    
    class GameController : IGameModeController
    {
        public void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
        }

        public void CleanupPlayer(Guid sessionId)
        {
        }

        public bool IsGameFinished()
        {
            return true;
        }

        public TimeSpan GetGameTtl()
        {
            return TimeSpan.FromHours(1);
        }

        public void Cleanup()
        {
        }

        public MessageResult ProcessMessage(MessageData message, Guid sessionId)
        {
            return new MessageResult {Handled = false};
        }
    }

    class GameControllerFactory : IGameModeControllerFactory
    {
        public IGameModeController GetGameModeController(IRoom room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new GameController();
        }
    }

    class GameBundle : IGameBundle
    {
        public IGameModeControllerFactory GetGameModeControllerFactory()
        {
            return new GameControllerFactory();
        }

        public void OnInitialize(IShamanComponents shamanComponents)
        {
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            StandaloneServerLauncher.Launch(new GameBundle(), args, "TestGame", "SomeRegion", "localhost",
                new List<ushort> {23453}, 7005);
        }
    }
}