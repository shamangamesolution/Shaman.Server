using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;

namespace Shaman.Client.TestBundle
{
    public class Game:IGameBundle
    {
        public IRoomControllerFactory GetRoomControllerFactory()
        {
            return new TestRoomControllerFactory();
        }

        public void OnInitialize(IShamanComponents shamanComponents)
        {
        }
    }

    class TestRoomControllerFactory:IRoomControllerFactory
    {
        public IRoomController GetGameModeController(IRoomContext room, ITaskScheduler taskScheduler,
            IRoomPropertiesContainer roomPropertiesContainer)
        {
            return new TestGameController();
        }
    }

    class TestGameController:IRoomController
    {
        public Task<bool> ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            return Task.FromResult(true);
        }

        public void ProcessPlayerDisconnected(Guid sessionId, PeerDisconnectedReason reason, byte[] reasonPayload)
        {
        }


        public bool IsGameFinished()
        {
            return false;
        }

        public TimeSpan ForceDestroyRoomAfter => TimeSpan.FromMinutes(5);
        public void ProcessMessage(Payload message, DeliveryOptions deliveryOptions, Guid sessionId)
        {
        }

        public void Dispose()
        {
        }
    }
}