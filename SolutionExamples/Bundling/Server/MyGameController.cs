using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shaman.Common.Utils.Serialization;
using Shaman.Game.Contract;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;

namespace Server
{
    internal class MyGameController : IGameModeController
    {
        private readonly ISerializer _serializer;
        private readonly IRoom _room;
        private readonly DateTime _gameStart;
        private int _players;

        public MyGameController(ISerializer serializer, IRoom room)
        {
            _serializer = serializer;
            _room = room;
            _gameStart = DateTime.UtcNow;
        }

        public Task<bool> ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            var currentCount = Interlocked.Increment(ref _players);
            if (currentCount > 12)
            {
                Interlocked.Decrement(ref _players);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public void CleanupPlayer(Guid sessionId)
        {
            Interlocked.Decrement(ref _players);
        }

        public bool IsGameFinished()
        {
            return DateTime.UtcNow - _gameStart > TimeSpan.FromSeconds(10) && _players == 0;
        }

        public void Cleanup()
        {
        }

        public void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId)
        {
            if (operationCode == CustomOperationCode.PingRequest)
            {
                var pingRequest =
                    _serializer.DeserializeAs<PingRequest>(message.Buffer, message.Offset, message.Length);
                _room.AddToSendQueue(new PingResponse() {SourceTicks = pingRequest.SourceTicks}, sessionId);
            }
            else if (operationCode == CustomOperationCode.Test)
            {
                _room.SendToAll(message, operationCode, message.IsReliable, false, sessionId);
            }
        }

        public TimeSpan ForceDestroyRoomAfter => TimeSpan.FromMinutes(2);
    }
}