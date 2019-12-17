using System;
using System.Collections.Generic;
using Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Game.Contract;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.Handling;

namespace Server
{
    class GameController : IGameModeController
    {
        private readonly IRoom _room;
        private readonly ISerializer _serializer;

        public GameController(IRoom room, IRoomPropertiesContainer roomPropertiesContainer, ISerializer serializer)
        {
            _room = room;
            _serializer = serializer;
        }

        public void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            _room.ConfirmedJoin(sessionId);
            _room.SendToAll(new PlayerEvent
            {
                PlayerId = sessionId, Action = PlayerAction.Joined
            }, sessionId);
        }

        public void CleanupPlayer(Guid sessionId)
        {
            _room.SendToAll(new PlayerEvent
            {
                PlayerId = sessionId, Action = PlayerAction.Leave
            }, sessionId);
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

        public MessageResult ProcessMessage(ushort operationCode, MessageData message, Guid sessionId)
        {
            Console.WriteLine($"Message from {sessionId}: {operationCode} in room {_room.GetRoomId()}.");
            if (operationCode == CustomOperationCode.PingRequest)
            {
                var pingRequest = _serializer.DeserializeAs<PingRequest>(message.Buffer, message.Offset,
                    message.Length);

                _room.SendToAll(new PingResponse() {SourceTicks = pingRequest.SourceTicks}, sessionId);
                return new MessageResult
                {
                    DeserializedMessage = pingRequest,
                    Handled = true
                };
            }

            if (operationCode == MessageCodes.CustomEvent)
            {
                return new MessageResult
                {
                    DeserializedMessage =
                        _serializer.DeserializeAs<CustomEvent>(message.Buffer, message.Offset,
                            message.Length),
                    Handled = true
                };
            }

            throw new NotSupportedException($"Unsupported message code {operationCode}");
        }
    }
}