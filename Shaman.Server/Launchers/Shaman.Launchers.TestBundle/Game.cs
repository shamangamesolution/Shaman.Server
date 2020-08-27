using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shaman.Contract.Bundle;
using Shaman.Contract.Bundle.DI;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Launchers.TestBundle
{
    public class Game : GameBundleBase
    {
        protected override void OnConfigureServices(IServiceCollection serviceCollection)
        {
            try
            {
                //singletons
                serviceCollection.AddSingleton<IRoomControllerFactory, TestRoomControllerFactory>();
                var provider = serviceCollection.BuildServiceProvider();
                var value = provider.GetService<IBundleConfig>().GetValueOrNull("CustomSetting");
                provider.GetService<IShamanLogger>().Error($"Received config value: {value}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected override void OnStart(IServiceProvider serviceProvider)
        {
            //start
        }
    }

    class TestRoomControllerFactory : IRoomControllerFactory
    {
        private readonly IShamanLogger _logger;

        public TestRoomControllerFactory(IShamanLogger logger)
        {
            _logger = logger;
            _logger.Error("Room factory created");
        }

        public IRoomController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new TestGameController(_logger, room);
        }
    }

    class TestGameController : IRoomController
    {
        private readonly IShamanLogger _logger;
        private readonly IRoomContext _room;

        public TestGameController(IShamanLogger logger, IRoomContext room)
        {
            _room = room;
            _logger = logger;
            Trace("Room crated");
        }

        public Task<bool> ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            Trace($"Player with sid {sessionId} joined");
            return Task.FromResult(true);
        }

        public void ProcessPlayerDisconnected(Guid sessionId, PeerDisconnectedReason reason, byte[] reasonPayload)
        {
            Trace($"Player with sid {sessionId} disconnected because {reason}");
        }


        public bool IsGameFinished()
        {
            return false;
        }

        public TimeSpan ForceDestroyRoomAfter => TimeSpan.FromMinutes(5);

        public void ProcessMessage(Payload message, DeliveryOptions deliveryOptions, Guid sessionId)
        {
            var operationCode = MessageBase.GetOperationCode(message.Buffer, message.Offset);
            Trace($"Message {operationCode} received");
        }

        public void Dispose()
        {
            Trace("Room disposing...");
        }

        [Conditional("DEBUG")]
        public void Trace(string message)
        {
            _logger.Error($"{_room.GetRoomId()}: {message}");
        }
    }
}