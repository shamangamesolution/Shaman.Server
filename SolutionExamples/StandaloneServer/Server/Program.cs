using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game;
using Shaman.Game.Contract;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.Handling;

namespace Server
{
    class GameController : IGameModeController
    {
        private readonly IRoom _room;

        public GameController(IRoom room, IRoomPropertiesContainer roomPropertiesContainer)
        {
            _room = room;
        }

        public void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            Console.WriteLine("ProcessNewPlayer: sessionId = {0}", sessionId);
            _room.ConfirmedJoin(sessionId);
        }

        public void CleanupPlayer(Guid sessionId)
        {
            Console.WriteLine("CleanupPlayer: sessionId = {0}", sessionId);
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
            Console.WriteLine("Cleanup");
        }

        public MessageResult ProcessMessage(MessageData message, Guid sessionId)
        {
            var operationCode = MessageBase.GetOperationCode(message.Buffer, message.Offset);
            Console.WriteLine($"Message from {sessionId}: {operationCode}");
            return new MessageResult
            {
                DeserializedMessage = new PingRequest(),
                Handled = true
            };
        }
    }

    class GameControllerFactory : IGameModeControllerFactory
    {
        public IGameModeController GetGameModeController(IRoom room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new GameController(room, roomPropertiesContainer);
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