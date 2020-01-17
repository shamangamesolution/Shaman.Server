using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Game.Providers;
using Shaman.Messages;
using Shaman.Messages.MM;
using RoomStats = Shaman.Game.Contract.Stats.RoomStats;

namespace Shaman.Game.Rooms
{
    public class Room : IRoom
    {
        private readonly IShamanLogger _logger;
        private readonly ConcurrentDictionary<Guid, RoomPlayer> _roomPlayers = new ConcurrentDictionary<Guid, RoomPlayer>();

        private readonly Guid _roomId;
        private readonly DateTime _createdOn;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IRoomManager _roomManager;
        private readonly ISerializer _serializer;
        private readonly IRoomPropertiesContainer _roomPropertiesContainer;
        private readonly IPacketSender _packetSender;
        private readonly IGameModeController _gameModeController;
        private readonly RoomStats _roomStats;
        private readonly IRequestSender _requestSender;

        private RoomState _roomState = RoomState.Closed;
        
        public Room(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IRoomManager roomManager, ISerializer serializer,
            IRoomPropertiesContainer roomPropertiesContainer,
            IGameModeControllerFactory gameModeControllerFactory, IPacketSender packetSender, IRequestSender requestSender)
        {
            _logger = logger;
            _roomId = Guid.NewGuid();
            _createdOn = DateTime.UtcNow;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _roomManager = roomManager;
            _serializer = serializer;
            _roomPropertiesContainer = roomPropertiesContainer;
            _packetSender = packetSender;
            _requestSender = requestSender;

            _roomStats = new RoomStats(GetRoomId(), roomPropertiesContainer.GetPlayersCount());
            
            _gameModeController =
                gameModeControllerFactory.GetGameModeController(
                    this, _taskScheduler, roomPropertiesContainer);

            _taskScheduler.ScheduleOnInterval(() =>
            {
                var maxQueueSIze = _packetSender.GetMaxQueueSIze();
//                if (maxQueueSIze > 3)
//                {
//                    _logger.Error($"MAX SEND QUEUE: {maxQueueSIze}");
//                }
                _roomStats.AddMaxQueueSize(maxQueueSIze);
                _roomStats.AddAvgQueueSize(_packetSender.GetAverageQueueSize());
                
            }, 0, 1000, true);
        }

        public TimeSpan GetRoomTtl()
        {
            return _gameModeController.GetGameTtl();
        }

        public void UpdateRoom(Dictionary<Guid, Dictionary<byte, object>> players)
        {
            _roomPropertiesContainer.AddNewPlayers(players);
        }

        public void Open()
        {
            _roomState = RoomState.Open;
            //update state on matchmaker
            SendRoomStateUpdate();
        }

        public void Close()
        {
            _roomState = RoomState.Closed;
            //update state on matchmaker
            SendRoomStateUpdate();
        }

        public void SendRoomStateUpdate()
        {
            try
            {
                var matchMakerUrl =
                    _roomPropertiesContainer.GetRoomPropertyAsString(PropertyCode.RoomProperties.MatchMakerUrl);

                if (string.IsNullOrWhiteSpace(matchMakerUrl))
                {
                    _logger.Error($"SendRoomStateUpdate error: matchmaker URL is empty in properties container");
                    return;
                }

                _requestSender.SendRequest<UpdateRoomStateResponse>(matchMakerUrl,
                    new UpdateRoomStateRequest(GetRoomId(), _roomPlayers.Count, _roomState), (r) =>
                    {
                        if (!r.Success)
                        {
                            _logger.Error($"Room update error: {r.Message}");
                        }
                        else
                        {
                            _logger.Debug($"Room update to {matchMakerUrl} with players count {_roomPlayers.Count}, state {_roomState} successful");
                        }
                    });
            }
            catch (Exception e)
            {
                _logger.Error($"Update room state error: {e}");
            }
        }

        public void SendToAll(MessageBase message, params Guid[] exceptions)
        {
            if (exceptions == null)
                exceptions = Array.Empty<Guid>();

            var initMsgArray = _serializer.Serialize(message);

            foreach (var player in _roomPlayers.Where(p => exceptions.All(e => e != p.Value.Peer.GetSessionId())))
            {
                AddToSendQueue(initMsgArray, player.Value.Peer, message.IsReliable, message.IsOrdered);
                //add to stats
                _roomStats.TrackSentMessage(initMsgArray.Length, message.IsReliable, message.OperationCode);
            }
        }

        public void SendToAll(MessageData messageData, ushort opCode, Guid sessionId, bool isReliable, bool isOrdered,
            params Guid[] exceptions)
        {
            if (exceptions == null)
                exceptions = Array.Empty<Guid>();

            foreach (var player in _roomPlayers.Where(p => exceptions.All(e => e != p.Value.Peer.GetSessionId())))
            {
                RoomPlayer player1 = player.Value;
                AddToSendQueue(messageData, opCode, isReliable,isOrdered, player1.Peer);
                //add to stats
                _roomStats.TrackSentMessage(messageData.Length, isReliable, opCode);
            }
        }
        public IEnumerable<RoomPlayer> GetAllPlayers()
        {
            return _roomPlayers.Values;
        }

        public void ConfirmedJoin(Guid sessionId)
        {
            _roomManager.ConfirmedJoin(sessionId, this);
            //send update
            SendRoomStateUpdate();
        }

        public RoomStats GetStats()
        {
            return _roomStats;
        }

        public bool IsGameFinished()
        {
            return _gameModeController.IsGameFinished();
        }

        public Guid GetRoomId()
        {
            return _roomId;
        }

        public bool PeerJoined(IPeer peer, Dictionary<byte, object> peerProperties)
        {
            if (!_roomPlayers.TryAdd(peer.GetSessionId(), new RoomPlayer(peer, peerProperties)))
            {
                _logger.Error($"Error adding player to peer collection");
                return false;
            }

            try
            {
                //find player and set wasjoined
                _gameModeController?.ProcessNewPlayer(peer.GetSessionId(), peerProperties);
                if (_gameModeController == null)
                {
                    _logger.Error($"GameModeController == null while peer joining");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"PeerJoined error for player with sessionId = {peer.GetSessionId()}: {ex}");
                return false;
            }

        }

        public void PeerLeft(Guid sessionId)
        {
            _roomPlayers.TryRemove(sessionId, out var player);
            try
            {
                _gameModeController.CleanupPlayer(sessionId);
            }
            catch (Exception ex)
            {
                _logger.Error($"CleanUpPlayer error: {ex}");
            }
            
            //send update
            SendRoomStateUpdate();
        }

        public void PeerDisconnected(Guid sessionId)
        {
            _roomPlayers.TryRemove(sessionId, out var player);
            try
            {
                _gameModeController.CleanupPlayer(sessionId);
            }
            catch (Exception ex)
            {
                _logger.Error($"CleanUpPlayer error: {ex}");
            }
            //send update
            SendRoomStateUpdate();
        }

        public void AddToSendQueue(MessageBase message, Guid sessionId)
        {
            if (!_roomPlayers.ContainsKey(sessionId))
                return;
            
            var player = _roomPlayers[sessionId];
            if (player != null)
            {
                var serialized = _serializer.Serialize(message);
                AddToSendQueue(serialized, player.Peer, message.IsReliable, message.IsOrdered);
                //add to stats
                _roomStats.TrackSentMessage(serialized.Length, message.IsReliable, message.OperationCode);
                //_taskScheduler.ScheduleOnceOnNow(() => player.Peer.Send(serialized, ));
            }
            else
            {
                _logger.Error($"Trying to send message {message.GetType()} to non-existing player {sessionId}");
            }
        }
        
        public void AddToSendQueue(MessageData messageData, ushort opCode, Guid sessionId, bool isReliable, bool isOrdered)
        {
            if (!_roomPlayers.ContainsKey(sessionId))
                return;
            
            var player = _roomPlayers[sessionId];
            if (player != null)
            {
                AddToSendQueue(messageData, opCode, isReliable, isOrdered, player.Peer);
            }
            else
            {
                _logger.Error($"Trying to send message with code {opCode} to non-existing player {sessionId}");
            }
        }

        private void AddToSendQueue(MessageData messageData, ushort opCode, bool isReliable, bool isOrdered, IPeer playerPeer)
        {
            _taskScheduler.ScheduleOnceOnNow(() =>
            {
                _packetSender.AddPacket(playerPeer, messageData.Buffer, messageData.Offset, messageData.Length, isReliable, isOrdered);
            });

            //add to stats
            _roomStats.TrackSentMessage(messageData.Length, isReliable, opCode);
        }

        private void AddToSendQueue(byte[] bytes, IPeer peer, bool isReliable, bool isOrdered)
        {
            //_taskScheduler.ScheduleOnceOnNow(() => peer.Send(bytes, isReliable, isOrdered));
            _taskScheduler.ScheduleOnceOnNow(() =>
            {
                _packetSender.AddPacket(peer, bytes, isReliable, isOrdered);
            });
        }
        
        public void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId)
        {
            try
            {
                var result = _gameModeController.ProcessMessage(operationCode, message, sessionId);
                var deserializedMessage = result.DeserializedMessage;
                _roomStats.TrackReceivedMessage(operationCode, message.Length, deserializedMessage.IsReliable);
                if (result.Handled && deserializedMessage.IsBroadcasted)
                {
                    // todo cannot pass initial array because in may be in rent and sending buffer filling in separate thread
                    SendToAll(deserializedMessage, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Room.ProcessMessage: Error processing {operationCode} message: {ex}");
            }
        }

        public void CleanUp()
        {
            try
            {
                foreach (var player in _roomPlayers)
                {
                    player.Value.Peer.Disconnect(DisconnectReason.RoomCleanup);
                }

                _packetSender.Stop();
                _gameModeController.Cleanup();
                _taskScheduler.RemoveAll();
                _roomPlayers.Clear();
                //close room on matchmaker
                Close();
            }
            catch (Exception e)
            {
                _logger?.Error($"Error disposing room: {e}");
            }
            finally
            {
                _logger?.Error($"RoomStats: {_roomStats}");
            }
        }

        public int GetPeerCount()
        {
            return _roomPlayers.Count;
        }

        public DateTime GetCreatedOnDateTime()
        {
            return _createdOn;
        }

        public RoomPlayer GetPlayer(Guid sessionId)
        {            
            _roomPlayers.TryGetValue(sessionId, out var player);
            return player;
        }
    }
}