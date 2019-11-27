using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Game.Contract;
using Shaman.GameBundleContract;

namespace Game.Bundle
{
    internal class MyGameController : IGameModeController
    {
        private readonly ISerializer _serializer;

        public MyGameController(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
        }

        public void CleanupPlayer(Guid sessionId)
        {
        }

        public bool IsGameFinished()
        {
            return false;
        }

        public TimeSpan GetGameTtl()
        {
            return TimeSpan.FromMinutes(10);
        }

        public void Cleanup()
        {
        }

        public MessageResult ProcessMessage(MessageData message, Guid sessionId)
        {
            var myMessage = _serializer.DeserializeAs<MyMessage>(message.Buffer, message.Offset, message.Length);
            return new MessageResult
            {
                Handled = false,
                DeserializedMessage = myMessage
            };
        }
    }
}