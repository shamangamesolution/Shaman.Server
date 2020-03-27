using System;
using System.Collections.Generic;
using Sample.Shared.Data;
using Sample.Shared.Data.DTO.Requests;
using Sample.Shared.Data.DTO.Responses;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Entity.Gameplay;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Extensions;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Messages;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.Handling;
using Shaman.Messages.RoomFlow;

namespace Sample.Game.GamePlay.Controllers
{
    public abstract class GameModeControllerBase : IGameModeController
    {
        protected readonly ISerializer SerializerFactory;
        protected readonly IRequestSender RequestSender;
        protected readonly IRoom Room;
        protected readonly IShamanLogger Logger;
        protected ITaskScheduler TaskScheduler;
        protected IStorageContainer StorageContainer;
        protected IRoomPropertiesContainer RoomPropertiesContainer;
        protected IBackendProvider BackendProvider;
        private PendingTask _gameTimerTask, _checkStartGameTask;
        private object _syncStartGame = new object();
        
        public GameModeControllerBase(IRoom room, IShamanLogger logger, IRequestSender requestSender,
            ITaskScheduler taskScheduler, IStorageContainer storageContainer,
            IRoomPropertiesContainer roomPropertiesContainer, IBackendProvider backendProvider, ISerializer serializerFactory)
        {
            Room = room;
            Logger = logger;
            RequestSender = requestSender;
            TaskScheduler = taskScheduler;
            StorageContainer = storageContainer;
            RoomPropertiesContainer = roomPropertiesContainer;
            BackendProvider = backendProvider;
            SerializerFactory = serializerFactory;
            
            if (!roomPropertiesContainer.IsRoomPropertiesContainsKey(PropertyCode.RoomProperties.GameMode))
                throw new Exception($"GameModeControllerBase.ctr error: no GameMode in property container");
            var mode = (GameMode)roomPropertiesContainer.GetRoomProperty<byte>(PropertyCode.RoomProperties.GameMode).Value;
            
            //subscribe waiter
            _checkStartGameTask = TaskScheduler.ScheduleOnInterval(() =>
            {
                if (CheckStartConditions())
                {
                    StartGame();
                    taskScheduler.Remove(_checkStartGameTask);
                }
            }, 0, 1000);
        }

        public abstract void ProcessNewPlayer(Player player, Guid sessionId, int backendId);
        public abstract bool ProcessCharacterSpawn(int playerIndex);

        public abstract void ProcessDeadCharacter(int playerIndex);

        public void ProcessNewPlayer(Guid sessionId, Dictionary<byte, object> properties)
        {
            var backendId = properties.GetInt(PropertyCode.PlayerProperties.BackendId);

            RequestSender.SendRequest<GetPlayerGameDataResponse>(
                BackendProvider.GetBackendUrl(backendId),
                new GetPlayerGameDataRequest
                {
                    SessionId = sessionId
                },
                (response) =>
                {
                    if (response == null)
                    {
                        Logger.Error($"GetPlayerGameDataResponse is null for player {sessionId}");
                        Room.AddToSendQueue(new JoinRoomResponse() {ResultCode = ResultCode.SendRequestError}, sessionId);
                        return;
                    }

                    if (!response.Success)
                    {
                        Logger.Error($"GetPlayerGameDataResponse error (adding player {sessionId} from backend {backendId}): result code {response.ResultCode}, message {response.Message}");
                        Room.AddToSendQueue(new JoinRoomResponse() {ResultCode = ResultCode.SendRequestError}, sessionId);
                        return;
                    }
                    
                    Logger.Info($"Player {sessionId} added");
                    Room.AddToSendQueue(new JoinRoomResponse(), sessionId);
                    //perform game mode related logic
                    ProcessNewPlayer(response.Player, sessionId, backendId);
                    Room.ConfirmedJoin(sessionId);
                });
        }

        private void StartGame()
        {

        }
        
        protected virtual bool CheckStartConditions()
        {
            return false;
        }
        
        public void CleanupPlayer(Guid sessionId)
        {
        }

        public bool IsGameFinished()
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetGameTtl()
        {
            throw new NotImplementedException();
        }

        public void Cleanup()
        {
        }

        public MessageResult ProcessMessage(ushort operationCode, MessageData message, Guid sessionId)
        {
            try
            {
                var deserMessage =
                    MessageFactory.DeserializeMessage(operationCode, new BinarySerializer(), message.Buffer, message.Offset, message.Length);
                
                //process room message
                switch (operationCode)
                {
                    case CustomOperationCode.PingRequest:
                        var pingRequest = deserMessage as PingRequest;
                        Room.AddToSendQueue(new PingResponse {SourceTicks = pingRequest.SourceTicks}, sessionId);
                        break;
                }
                
                return new MessageResult
                {
                    DeserializedMessage = deserMessage,
                    Handled = false
                };
                    
            }
            catch (Exception ex)
            {
                throw new MessageProcessingException(
                    $"Error processing with code {operationCode}", ex);
            }
        }
        
        public void CleanUp()
        {
            TaskScheduler.Remove(_gameTimerTask);
        }
    }
}